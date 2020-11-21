using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
        private const bool _ENABLE_BACK_FACE_CULLING = false;
        private const bool _ENABLE_FRUSTUM_CULLING = true;

        private const int _BUILT_IN_VIEWPORT_UNIFORMS_OFFSET = 0;
        private const int _BUILT_IN_VIEWPORT_UNIFORMS_SIZE = 16;
        private const int _BUILT_IN_CAMERA_UNIFORMS_OFFSET = 16;
        private const int _BUILT_IN_CAMERA_UNIFORMS_SIZE = 144;
        private const int _BUILT_IN_MODEL_UNIFORMS_OFFSET = 160;
        private const int _BUILT_IN_MODEL_UNIFORMS_SIZE = 192;
        private const int _BUILT_IN_UNIFORMS_SIZE = _BUILT_IN_VIEWPORT_UNIFORMS_SIZE + _BUILT_IN_CAMERA_UNIFORMS_SIZE + _BUILT_IN_MODEL_UNIFORMS_SIZE;
        private readonly UniformBufferObject _BuiltInUniforms;

        private readonly GL _GL;

        private float _NewAspectRatio;

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

            _BuiltInUniforms = new UniformBufferObject(_GL, 0u, (uint)_BUILT_IN_UNIFORMS_SIZE);
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

        [SkipLocalsInit, HandledComponents(DistinctionStrategy.All, typeof(Camera))]
        public override unsafe ValueTask UpdateAsync(EntityManager entityManager, TimeSpan delta)
        {
            _GL.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
            Span<Plane> planes = stackalloc Plane[Frustum.TOTAL_PLANES];
            Span<byte> cameraUniforms = stackalloc byte[sizeof(Vector4) + (2 * sizeof(Matrix4x4))];
            Span<byte> modelUniforms = stackalloc byte[3 * sizeof(Matrix4x4)];
            DrawCalls = 0;

            if (IsNewViewport())
            {
                UpdateViewport();
            }

            // ensure we bind the builtins UBO every frame
            _BuiltInUniforms.Bind();

            foreach ((IEntity cameraEntity, Camera camera) in entityManager.GetEntitiesWithComponents<Camera>())
            {
                // check for changes and update current camera's view matrix & UBO data
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

                // write camera uniforms to built-in UBO
                WriteCameraUniforms(cameraUniforms, camera.Projection.Parameters, camera.Projection.Matrix, camera.View);
                _BuiltInUniforms.Write(_BUILT_IN_CAMERA_UNIFORMS_OFFSET, cameraUniforms);

                // iterate each RenderMesh and check if the model matrix needs to be recalculated
                foreach ((IEntity objectEntity, RenderModel renderModel) in entityManager.GetEntitiesWithComponents<RenderModel>())
                {
                    CheckUpdateModelTransforms(objectEntity, renderModel);
                }

                // bind camera's view data UBO and precalculate viewproj matrix
                Matrix4x4 viewProjection = camera.View * camera.Projection.Matrix;
                Material? cachedMaterial = null;

                // iterate every valid entity and try to render it
                // we also sort the entities by their render pipeline ID, so we can avoid doing a ton of rebinding
                foreach ((IEntity objectEntity, RenderMesh renderMesh, Material material) in GetRenderableEntities(entityManager, camera))
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
                    WriteModelUniforms(modelUniforms, modelViewProjection, modelInverted, model);
                    _BuiltInUniforms.Write(_BUILT_IN_MODEL_UNIFORMS_OFFSET, modelUniforms);

                    renderMesh.Mesh!.Draw();
                    DrawCalls += 1;
                }
            }

            // by this point we should've updated all of the projection matrixes, so reset the value to 0.
            _NewAspectRatio = 0f;

            return ValueTask.CompletedTask;
        }

        private static IEnumerable<(IEntity, RenderMesh, Material)> GetRenderableEntities(EntityManager entityManager, Camera camera) =>
            entityManager.GetEntitiesWithComponents<RenderMesh, Material>()
                .Where(result => result.Component1.ShouldRender && ((camera.RenderedLayers & result.Component1.Mesh!.Layer) > 0))
                .OrderBy(result => result.Component2.Pipeline.Handle);

        private bool IsNewViewport() => _NewAspectRatio is not 0f;


        #region Occlusion

        private static bool CheckClipFrustumOccludeEntity(IEntity entity, Span<Plane> planes, Matrix4x4 mvp)
        {
            if (!entity.TryFind(out OcclusionBounds? bounds) || !_ENABLE_FRUSTUM_CULLING)
            {
                return false;
            }

            ClipFrustum frustum = new(planes, mvp);
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


        #region Update Model

        private static void CheckUpdateModelTransforms(IEntity objectEntity, RenderModel renderModel)
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
            _BuiltInUniforms.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable


        #region Uniforms

        private void UpdateViewport()
        {
            Vector4 viewport = new(0f, 0f, AutomataWindow.Instance.Size.X, AutomataWindow.Instance.Size.Y);
            _BuiltInUniforms.Write(_BUILT_IN_VIEWPORT_UNIFORMS_OFFSET, viewport);
        }

        private static void WriteCameraUniforms(Span<byte> destination, Vector4 parameters, Matrix4x4 projection, Matrix4x4 view)
        {
            const int minimum_destination_size = 144;
            const int projection_offset = 16;
            const int view_offset = 80;

            if (destination.Length < minimum_destination_size)
            {
                throw new ArgumentOutOfRangeException(nameof(destination), $"Destination span must be at least {minimum_destination_size} bytes.");
            }

            MemoryMarshal.Write(destination, ref parameters);
            MemoryMarshal.Write(destination.Slice(projection_offset), ref projection);
            MemoryMarshal.Write(destination.Slice(view_offset), ref view);
        }

        private static void WriteModelUniforms(Span<byte> destination, Matrix4x4 mvp, Matrix4x4 obj, Matrix4x4 world)
        {
            const int minimum_destination_size = 192;
            const int object_offset = 64;
            const int world_offset = 128;

            if (destination.Length < minimum_destination_size)
            {
                throw new ArgumentOutOfRangeException(nameof(destination), $"Destination span must be at least {minimum_destination_size} bytes.");
            }

            MemoryMarshal.Write(destination, ref mvp);
            MemoryMarshal.Write(destination.Slice(object_offset), ref obj);
            MemoryMarshal.Write(destination.Slice(world_offset), ref world);
        }

        #endregion


        #region Update Camera

        private static void CheckUpdateCameraView(IEntity cameraEntity, Camera camera)
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
    }
}
