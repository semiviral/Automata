using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Serilog;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Plane = Automata.Engine.Numerics.Shapes.Plane;

namespace Automata.Engine.Rendering
{
    public class RenderSystem : ComponentSystem, IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        private readonly struct CameraUniforms
        {
            public readonly Vector4 Viewport;
            public readonly Vector4 Parameters;
            public readonly Matrix4x4 Projection;
            public readonly Matrix4x4 View;

            public CameraUniforms(Vector4 viewport, Vector4 parameters, Matrix4x4 projection, Matrix4x4 view)
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

        public unsafe RenderSystem(World world) : base(world)
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
            _ViewUniforms = new RingBufferObject(_GL, (nuint)sizeof(CameraUniforms), 3u, (nuint)alignment);
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
        public override unsafe ValueTask UpdateAsync(EntityManager entityManager, TimeSpan delta)
        {
            _GL.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
            Span<Plane> planes = stackalloc Plane[Frustum.TOTAL_PLANES];
            Interlocked.Exchange(ref _DrawCalls, 0u);

            foreach (Camera camera in entityManager.GetComponents<Camera>())
            {
                // if the camera doesn't have a projection, it doesn't make sense to try and render to it
                // remark: this may possible be an incorrect assumption, but for now it makes sense
                if (camera.Projection is null)
                {
                    continue;
                }

                CameraUniforms camera_uniforms = new CameraUniforms(AutomataWindow.Instance.Viewport, camera.Projection.Parameters, camera.Projection.Matrix,
                    camera.View);

                _ViewUniforms.Write(ref camera_uniforms);
                _ViewUniforms.Bind(BufferTargetARB.UniformBuffer, 0u);
                DrawModels(entityManager, camera, planes);
                _ViewUniforms.FenceRing();
            }

            return ValueTask.CompletedTask;
        }

        private void DrawModels(EntityManager entityManager, Camera camera, Span<Plane> planes)
        {
            Debug.Assert(camera.Projection is not null, "This should be verified outside this method.");

            Matrix4x4 view_projection = camera.View * camera.Projection.Matrix;
            Material? cached_material = null;

            // iterate every valid entity and try to render it
            // we also sort the entities by their render pipeline ID, so we can avoid doing a ton of rebinding
            foreach ((Entity entity, RenderMesh render_mesh, Material material) in GetRenderableEntities(entityManager, camera))
            {
                Matrix4x4 model = entity.Component<Transform>()?.Matrix ?? Matrix4x4.Identity;
                Matrix4x4 model_view_projection = model * view_projection;

                if (CheckClipFrustumOccludeEntity(entity, planes, model_view_projection))
                {
                    continue;
                }

                if (!material.Equals(cached_material))
                {
                    ApplyMaterial(material);
                    cached_material = material;
                }

                Matrix4x4.Invert(model, out Matrix4x4 model_inverted);
                ModelUniforms model_uniforms = new ModelUniforms(model_view_projection, model_inverted, model);
                _ModelUniforms.Write(ref model_uniforms);
                _ModelUniforms.Bind(BufferTargetARB.UniformBuffer, 1u);

#if DEBUG
                _GL.ValidateProgramPipeline(material.Pipeline.Handle);

                if (material.Pipeline.TryGetInfoLog(out string? infoLog))
                {
                    Log.Error(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(RenderSystem), infoLog));
                }
#endif

                render_mesh.Mesh!.Draw();
                _ModelUniforms.FenceRing();
                Interlocked.Increment(ref _DrawCalls);
            }
        }

        private static IEnumerable<(Entity, RenderMesh, Material)> GetRenderableEntities(EntityManager entityManager, Camera camera) =>
            entityManager.GetEntitiesWithComponents<RenderMesh, Material>()
                .Where(result => result.Component1.ShouldRender && ((camera.RenderedLayers & result.Component1.Mesh!.Layer) > 0))
                .OrderBy(result => result.Component2.Pipeline.Handle);


        #region Occlusion

        private static bool CheckClipFrustumOccludeEntity(Entity entity, Span<Plane> planes, Matrix4x4 mvp)
        {
            if (!_ENABLE_FRUSTUM_CULLING || !entity.TryComponent(out OcclusionBounds? bounds))
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
            ShaderProgram fragment_shader = material.Pipeline.Stage(ShaderType.FragmentShader);
            int index = 0;

            foreach (string key in material.Textures.Keys)
            {
                fragment_shader.TrySetUniform($"tex_{key}", index);
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
