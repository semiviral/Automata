using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Automata.Engine.Input;
using Automata.Engine.Numerics.Shapes;
using Automata.Engine.Rendering.Meshes;
using Automata.Engine.Rendering.OpenGL;
using Automata.Engine.Rendering.OpenGL.Buffers;
using Automata.Engine.Rendering.OpenGL.Shaders;
using Automata.Engine.Rendering.OpenGL.Textures;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Plane = Automata.Engine.Numerics.Shapes.Plane;

namespace Automata.Engine.Rendering
{
    public class RenderSystem : ComponentSystem, IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        private readonly struct ViewUniforms
        {
            public readonly Vector4 Viewport;
            public readonly Vector4 Parameters;
            public readonly Matrix4x4 Projection;
            public readonly Matrix4x4 View;

            public ViewUniforms(Vector4 viewport, Vector4 parameters, Matrix4x4 projection, Matrix4x4 view)
            {
                Viewport = viewport;
                Parameters = parameters;
                Projection = projection;
                View = view;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct ModelUniforms
        {
            public readonly Matrix4x4 MVP;
            public readonly Matrix4x4 Object;
            public readonly Matrix4x4 World;

            public ModelUniforms(Matrix4x4 mvp, Matrix4x4 o, Matrix4x4 world)
            {
                MVP = mvp;
                Object = o;
                World = world;
            }
        }

        private const bool _ENABLE_BACK_FACE_CULLING = false;
        private const bool _ENABLE_FRUSTUM_CULLING = true;

        private readonly GL _GL;
        private readonly RingBufferObject _ViewUniforms;
        private readonly RingBufferObject _ModelUniforms;

        private ulong _DrawCalls;
        public ulong DrawCalls => _DrawCalls;

        public unsafe RenderSystem()
        {
            _GL = GLAPI.Instance.GL;
            _GL.ClearColor(Color.DimGray);
            _GL.Enable(EnableCap.DepthTest);

            if (_ENABLE_BACK_FACE_CULLING)
            {
                // configure and enable face culling
                _GL.FrontFace(FrontFaceDirection.Ccw);
                _GL.CullFace(CullFaceMode.Back);
                _GL.Enable(EnableCap.CullFace);
            }

            _GL.GetInteger(GetPName.UniformBufferOffsetAlignment, out int alignment);
            _ViewUniforms = new RingBufferObject(_GL, (nuint)sizeof(ViewUniforms), 3u, (nuint)alignment);
            _ModelUniforms = new RingBufferObject(_GL, (nuint)sizeof(ModelUniforms), 8u, (nuint)alignment);
        }

        public override void Registered(EntityManager entityManager)
        {
            bool wireframe = false;

            InputManager.Instance.RegisterInputAction(() =>
            {
                if (wireframe)
                {
                    _GL.PolygonMode(GLEnum.FrontAndBack, PolygonMode.Fill);
                    wireframe = false;
                }
                else
                {
                    _GL.PolygonMode(GLEnum.FrontAndBack, PolygonMode.Line);
                    wireframe = true;
                }
            }, Key.F4);
        }

        [SkipLocalsInit, HandledComponents(EnumerationStrategy.All, typeof(Camera))]
        public override ValueTask UpdateAsync(EntityManager entityManager, TimeSpan delta)
        {
            _GL.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
            Span<Plane> planes = stackalloc Plane[Frustum.TOTAL_PLANES];
            Interlocked.Exchange(ref _DrawCalls, 0u);

            foreach (Camera camera in entityManager.GetComponents<Camera>())
            {
                // if the camera doesn't have a projection, it doesn't make sense to try and render to it
                if (camera.Projection is null)
                {
                    continue;
                }

                ViewUniforms viewUniforms = new ViewUniforms(AutomataWindow.Instance.Viewport, camera.Projection.Parameters, camera.Projection.Matrix,
                    camera.View);

                _ViewUniforms.Write(ref viewUniforms);
                _ViewUniforms.Bind(BufferTargetARB.UniformBuffer, 0u);

                Matrix4x4 viewProjection = camera.View * camera.Projection.Matrix;
                Material? cachedMaterial = null;

                // iterate every valid entity and try to render it
                // we also sort the entities by their render pipeline ID, so we can avoid doing a ton of rebinding
                foreach ((Entity objectEntity, RenderMesh renderMesh, Material material) in GetRenderableEntities(entityManager, camera))
                {
                    Matrix4x4 model = objectEntity.Find<RenderModel>()?.Model ?? Matrix4x4.Identity;
                    Matrix4x4 modelViewProjection = model * viewProjection;

                    if (CheckClipFrustumOccludeEntity(objectEntity, planes, modelViewProjection))
                    {
                        continue;
                    }

                    if (!material.Equals(cachedMaterial))
                    {
                        ApplyMaterial(material);
                        cachedMaterial = material;
                    }

                    Matrix4x4.Invert(model, out Matrix4x4 modelInverted);
                    ModelUniforms modelUniforms = new ModelUniforms(modelViewProjection, modelInverted, model);
                    _ModelUniforms.Write(ref modelUniforms);
                    _ModelUniforms.Bind(BufferTargetARB.UniformBuffer, 1u);

                    renderMesh.Mesh!.Draw();
                    _ModelUniforms.CycleRing();
                    Interlocked.Increment(ref _DrawCalls);
                }

                _ViewUniforms.CycleRing();
            }

            return ValueTask.CompletedTask;
        }

        private static IEnumerable<(Entity, RenderMesh, Material)> GetRenderableEntities(EntityManager entityManager, Camera camera) =>
            entityManager.GetEntitiesWithComponents<RenderMesh, Material>()
                .Where(result => result.Component1.ShouldRender && ((camera.RenderedLayers & result.Component1.Mesh!.Layer) > 0))
                .OrderBy(result => result.Component2.Pipeline.Handle);


        #region Occlusion

        private static bool CheckClipFrustumOccludeEntity(Entity entity, Span<Plane> planes, Matrix4x4 mvp)
        {
            if (!_ENABLE_FRUSTUM_CULLING || !entity.TryFind(out OcclusionBounds? bounds))
            {
                return false;
            }

            ClipFrustum frustum = new ClipFrustum(planes, mvp);
            Frustum.Intersect intersection = Frustum.Intersect.Outside;

            return

                // test spherical bounds
                ((bounds.Spheric != Sphere.Zero) && (intersection = frustum.Intersects(bounds.Spheric)) is Frustum.Intersect.Outside)

                // if spherical bounds occlusion fails (i.e. intersects) try cubic
                || (intersection is not Frustum.Intersect.Inside
                    && (bounds.Cubic != Cube.Zero)
                    && frustum.Intersects(bounds.Cubic) is Frustum.Intersect.Outside);
        }

        #endregion


        #region State Change

        private void ApplyMaterial(Material material)
        {
            material.Pipeline.Bind();
            Texture.BindMany(_GL, 0u, material.Textures.Values);
            ShaderProgram fragmentShader = material.Pipeline.Stage(ShaderType.FragmentShader);
            int index = 0;

            foreach (string key in material.Textures.Keys)
            {
                fragmentShader.TrySetUniform($"tex_{key}", index);
                index += 1;
            }
        }

        #endregion


        #region IDisposable

        public void Dispose()
        {
            _ViewUniforms.Dispose();
            _ModelUniforms.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
