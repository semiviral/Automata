using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Automata.Engine.Input;
using Automata.Engine.Numerics;
using Automata.Engine.Numerics.Shapes;
using Automata.Engine.Rendering.Meshes;
using Automata.Engine.Rendering.OpenGL;
using Automata.Engine.Rendering.OpenGL.Buffers;
using Automata.Engine.Rendering.OpenGL.Shaders;
using Automata.Engine.Rendering.OpenGL.Textures;
using Silk.NET.Input.Common;
using Silk.NET.OpenGL;
using Plane = Automata.Engine.Numerics.Shapes.Plane;

namespace Automata.Engine.Rendering
{
    public class RenderSystem : ComponentSystem, IDisposable
    {
        private readonly ref struct ViewUniforms
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

        private readonly ref struct ModelUniforms
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

        private const int _VIEW_UNIFORMS_SIZE = 160;
        private const int _MODEL_UNIFORMS_SIZE = 192;

        private readonly RingBufferObject _ViewUniforms;
        private readonly RingBufferObject _ModelUniforms;

        private readonly GL _GL;

        private float _NewAspectRatio;
        private Vector4 _Viewport;

        public ulong DrawCalls { get; private set; }

        public RenderSystem()
        {
            GameWindowResized(null!, AutomataWindow.Instance.Size);
            AutomataWindow.Instance.Resized += GameWindowResized;

            _GL = GLAPI.Instance.GL;
            _GL.ClearColor(Color.DimGray);
            _GL.Enable(EnableCap.DepthTest);

            if (_ENABLE_BACK_FACE_CULLING)
            {
                // enable and configure face culling
                _GL.FrontFace(FrontFaceDirection.Ccw);
                _GL.CullFace(CullFaceMode.Back);
                _GL.Enable(EnableCap.CullFace);
            }

            _ViewUniforms = new RingBufferObject(_GL, (nuint)_VIEW_UNIFORMS_SIZE, 3u);
            _ModelUniforms = new RingBufferObject(_GL, (nuint)_MODEL_UNIFORMS_SIZE, 6u);
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
            DrawCalls = 0;

            if (IsNewViewport())
            {
                _Viewport = new Vector4(0f, 0f, AutomataWindow.Instance.Size.X, AutomataWindow.Instance.Size.Y);
            }

            foreach ((Entity cameraEntity, Camera camera) in entityManager.GetEntitiesWithComponents<Camera>())
            {
                CheckUpdateCameraView(cameraEntity, camera);

                // if the aspect ratio has changed, update the current camera's projection matrix
                if (IsNewViewport())
                {
                    UpdateCameraProjection(camera);
                }

                // if the camera doesn't have a projection, it doesn't make sense to try and render to it
                if (camera.Projection is null)
                {
                    continue;
                }

                ViewUniforms viewUniforms = new ViewUniforms(_Viewport, camera.Projection.Parameters, camera.Projection.Matrix, camera.View);
                _ViewUniforms.Write(&viewUniforms);
                _ViewUniforms.Bind(BufferTargetARB.UniformBuffer, 0u);
                _ViewUniforms.CycleRing();

                // iterate each RenderMesh and check if the model matrix needs to be recalculated
                foreach ((Entity objectEntity, RenderModel renderModel) in entityManager.GetEntitiesWithComponents<RenderModel>())
                {
                    CheckUpdateModelTransforms(objectEntity, renderModel);
                }

                // bind camera's view data UBO and precalculate viewproj matrix
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

                    // conditionally update the currentMaterial if it doesn't match this entity's
                    if (cachedMaterial is null || !material.Equals(cachedMaterial))
                    {
                        ApplyMaterial(material, ref cachedMaterial);
                    }

                    Matrix4x4.Invert(model, out Matrix4x4 modelInverted);
                    ModelUniforms modelUniforms = new ModelUniforms(modelViewProjection, modelInverted, model);
                    _ModelUniforms.Write(&modelUniforms);
                    _ModelUniforms.Bind(BufferTargetARB.UniformBuffer, 1u);

                    renderMesh.Mesh!.Draw();
                    _ModelUniforms.CycleRing();
                    DrawCalls += 1;
                }
            }

            // by this point we should've updated all of the projection matrixes, so reset the value to 0.
            _NewAspectRatio = 0f;

            return ValueTask.CompletedTask;
        }

        private static IEnumerable<(Entity, RenderMesh, Material)> GetRenderableEntities(EntityManager entityManager, Camera camera) =>
            entityManager.GetEntitiesWithComponents<RenderMesh, Material>()
                .Where(result => result.Component1.ShouldRender && ((camera.RenderedLayers & result.Component1.Mesh!.Layer) > 0))
                .OrderBy(result => result.Component2.Pipeline.Handle);

        private bool IsNewViewport() => _NewAspectRatio is not 0f;


        #region Occlusion

        private static bool CheckClipFrustumOccludeEntity(Entity entity, Span<Plane> planes, Matrix4x4 mvp)
        {
            if (!entity.TryFind(out OcclusionBounds? bounds) || !_ENABLE_FRUSTUM_CULLING)
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

        private void ApplyMaterial(Material material, ref Material? old)
        {
            if (material.Pipeline.Equals(old?.Pipeline))
            {
                return;
            }

            material.Pipeline.Bind();
            Texture.BindMany(_GL, 0u, material.Textures.Values);
            ShaderProgram newFragmentShader = material.Pipeline.Stage(ShaderType.FragmentShader);
            int index = 0;

            foreach (string key in material.Textures.Keys)
            {
                newFragmentShader!.TrySetUniform($"tex_{key}", index);
                index += 1;
            }

            // we've finished binding the new material, so replace the old's reference
            old = material;
        }

        #endregion


        #region Update Camera

        private static void CheckUpdateCameraView(Entity cameraEntity, Camera camera)
        {
            // check for changes and update current camera's view matrix & UBO data
            if (!(cameraEntity.TryFind(out Scale? cameraScale) && cameraScale.Changed)
                & !(cameraEntity.TryFind(out Rotation? cameraRotation) && cameraRotation.Changed)
                & !(cameraEntity.TryFind(out Translation? cameraTranslation) && cameraTranslation.Changed))
            {
                return;
            }

            Matrix4x4 view = Matrix4x4.Identity;

            if (cameraScale is not null)
            {
                view *= Matrix4x4.CreateScale(cameraScale.Value);
            }

            if (cameraRotation is not null)
            {
                view *= Matrix4x4.CreateFromQuaternion(cameraRotation.Value);
            }

            if (cameraTranslation is not null)
            {
                view *= Matrix4x4.CreateTranslation(cameraTranslation.Value);
            }

            // if we've calculated a new matrix, invert it and apply to camera
            if ((view != Matrix4x4.Identity) && Matrix4x4.Invert(view, out Matrix4x4 inverted))
            {
                camera.View = inverted;
            }
        }

        private void UpdateCameraProjection(Camera camera)
        {
            camera.Projection = camera.Projector switch
            {
                Projector.Perspective => new PerspectiveProjection(90f, _NewAspectRatio, 0.1f, 1000f),
                Projector.Orthographic => new OrthographicProjection(AutomataWindow.Instance.Size, 0.1f, 1000f),
                Projector.None or _ => camera.Projection
            };
        }

        #endregion


        #region Update Model

        private static void CheckUpdateModelTransforms(Entity objectEntity, RenderModel renderModel)
        {
            if ((!(objectEntity.TryFind(out Translation? modelTranslation) && modelTranslation.Changed)
                 & !(objectEntity.TryFind(out Rotation? modelRotation) && modelRotation.Changed)
                 & !(objectEntity.TryFind(out Scale? modelScale) && modelScale.Changed))
                && !renderModel.Changed)
            {
                return;
            }

            renderModel.Model = Matrix4x4.Identity;

            if (modelTranslation is not null)
            {
                renderModel.Model *= Matrix4x4.CreateTranslation(modelTranslation.Value);
            }

            if (modelRotation is not null)
            {
                renderModel.Model *= Matrix4x4.CreateFromQuaternion(modelRotation.Value);
            }

            if (modelScale is not null)
            {
                renderModel.Model *= Matrix4x4.CreateScale(modelScale.Value);
            }
        }

        #endregion


        #region Events

        private void GameWindowResized(object sender, Vector2i newSize) => _NewAspectRatio = (float)newSize.X / (float)newSize.Y;

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
