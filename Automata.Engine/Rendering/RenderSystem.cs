using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Automata.Engine.Components;
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

        private readonly GL _GL;
        private readonly UniformBufferObject _Viewport;

        private float _NewAspectRatio;

        public ulong DrawCalls { get; private set; }

        public unsafe RenderSystem()
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

            _Viewport = new UniformBufferObject(_GL, 1, (uint)sizeof(Vector4));
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

        [HandledComponents(DistinctionStrategy.All, typeof(Camera))]
        public override unsafe ValueTask Update(EntityManager entityManager, TimeSpan delta)
        {
            _GL.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
            Span<Plane> planes = stackalloc Plane[Frustum.TOTAL_PLANES];
            DrawCalls = 0;

            // update viewport UBO
            if (_NewAspectRatio > 0f)
            {
                _Viewport.Write(0, new Vector4(0f, 0f, AutomataWindow.Instance.Size.X, AutomataWindow.Instance.Size.Y));
            }

            _Viewport.Bind();

            foreach ((IEntity cameraEntity, Camera camera) in entityManager.GetEntitiesWithComponents<Camera>())
            {
                // allocate camera UBO if one doesn't exist
                camera.Uniforms ??= new UniformBufferObject(_GL, 0, (uint)(sizeof(Matrix4x4) + sizeof(Matrix4x4) + sizeof(Vector4)))
                {
                    ["view"] = 0,
                    ["projection"] = 64,
                    ["parameters"] = 128
                };

                // check for changes and update current camera's view matrix & UBO data
                if ((cameraEntity.TryFind(out Scale? cameraScale) && cameraScale.Changed)
                    | (cameraEntity.TryFind(out Rotation? cameraRotation) && cameraRotation.Changed)
                    | (cameraEntity.TryFind(out Translation? cameraTranslation) && cameraTranslation.Changed))
                {
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

                    camera.Uniforms.Write(0, camera.View);
                }

                // if the aspect ratio has changed, update the current camera's projection matrix & UBO data
                if (_NewAspectRatio > 0f)
                {
                    camera.Projection = camera.Projector switch
                    {
                        Projector.Perspective => new PerspectiveProjection(90f, _NewAspectRatio, 0.1f, 1000f),
                        Projector.Orthographic => new OrthographicProjection(AutomataWindow.Instance.Size, 0.1f, 1000f),
                        Projector.None or _ => camera.Projection
                    };

                    camera.Uniforms.Write(64, camera.Projection!.Matrix);
                    camera.Uniforms.Write(128, camera.Projection!.Parameters);
                }

                // if the camera doesn't have a projection, it doesn't make sense to try and render to it
                if (camera.Projection is null)
                {
                    continue;
                }

                // iterate each RenderMesh and check if the model matrix needs to be recalculated
                foreach ((IEntity objectEntity, RenderModel renderModel) in entityManager.GetEntitiesWithComponents<RenderModel>())
                {
                    if (((objectEntity.TryFind(out Translation? modelTranslation) && modelTranslation.Changed)
                         | (objectEntity.TryFind(out Rotation? modelRotation) && modelRotation.Changed)
                         | (objectEntity.TryFind(out Scale? modelScale) && modelScale.Changed))
                        || renderModel.Changed)
                    {
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
                }

                // bind camera's view data UBO and precalculate viewproj matrix
                camera.Uniforms.Bind();
                Matrix4x4 viewProjection = camera.View * camera.Projection.Matrix;
                Material? cachedMaterial = null;

                // iterate every valid entity and try to render it
                // we also sort the entities by their render pipeline ID, so we can avoid doing a ton of rebinding
                foreach ((IEntity objectEntity, RenderMesh renderMesh, Material material) in entityManager.GetEntitiesWithComponents<RenderMesh, Material>()
                    .Where(result => result.Component1.ShouldRender && ((camera.RenderedLayers & result.Component1.Mesh!.Layer) > 0))
                    .OrderBy(result => result.Component2.Pipeline.Handle))
                {
                    bool hasRenderModel = objectEntity.TryFind(out RenderModel? renderModel);
                    Matrix4x4 modelViewProjection = (hasRenderModel ? renderModel!.Model : Matrix4x4.Identity) * viewProjection;

                    if (objectEntity.TryFind(out OcclusionBounds? bounds) && CheckClipFrustumOcclude(bounds, planes, modelViewProjection))
                    {
                        continue;
                    }

                    // conditionally update the currentMaterial if it doesn't match this entity's
                    if (cachedMaterial is null || !material.Equals(cachedMaterial))
                    {
                        ApplyMaterial(material, ref cachedMaterial);
                    }

                    // we're about to render, so ensure all of the relevant uniforms are set
                    ShaderProgram vertexShader = cachedMaterial!.Pipeline.Stage(ShaderType.VertexShader);
                    vertexShader.TrySetUniform("_view", camera.View);
                    vertexShader.TrySetUniform("_proj", camera.Projection.Matrix);
                    vertexShader.TrySetUniform("_mvp", modelViewProjection);

                    if (hasRenderModel)
                    {
                        Matrix4x4.Invert(renderModel!.Model, out Matrix4x4 modelInverted);
                        vertexShader.TrySetUniform("_object", modelInverted);
                        vertexShader.TrySetUniform("_world", renderModel.Model);
                    }

                    renderMesh.Mesh!.Draw();
                    DrawCalls += 1;
                }
            }

            // by this point we should've updated all of the projection matrixes, so reset the value to 0.
            _NewAspectRatio = 0f;

            return ValueTask.CompletedTask;
        }

        private static bool CheckClipFrustumOcclude(OcclusionBounds occlusionBounds, Span<Plane> planes, Matrix4x4 mvp)
        {
            if (!_ENABLE_FRUSTUM_CULLING)
            {
                return false;
            }

            ClipFrustum frustum = new ClipFrustum(planes, mvp);
            Frustum.Intersect intersection = Frustum.Intersect.Outside;

            return

                // test spherical bounds
                ((occlusionBounds.Spheric != Sphere.Zero) && (intersection = frustum.Intersects(occlusionBounds.Spheric)) is Frustum.Intersect.Outside)

                // if spherical bounds occlusion fails (i.e. intersects) try cubic
                || (intersection is not Frustum.Intersect.Inside
                    && (occlusionBounds.Cubic != Cube.Zero)
                    && frustum.Intersects(occlusionBounds.Cubic) is Frustum.Intersect.Outside);
        }

        private static void ApplyMaterial(Material material, ref Material? old)
        {
            if (material.Pipeline.Equals(old?.Pipeline))
            {
                return;
            }

            material.Pipeline.Bind();

            // set fragment shader so we can bind textures quicker
            ShaderProgram newFragmentShader = material.Pipeline.Stage(ShaderType.FragmentShader);

            // if old isn't null, bind the old textures to the new pipeline
            if (old is not null)
            {
                foreach ((string key, Texture texture) in old.Textures)
                {
                    if (!material.Textures.ContainsKey(key))
                    {
                        material.Textures.Add(key, texture);
                    }
                }
            }

            int index = 0;

            foreach ((string key, Texture texture) in material.Textures)
            {
                texture.Bind(TextureUnit.Texture0 + index);
                newFragmentShader!.TrySetUniform($"tex_{key}", index);
                index += 1;
            }

            old = material;
        }

        private void GameWindowResized(object sender, Vector2i newSize) => _NewAspectRatio = (float)newSize.X / (float)newSize.Y;


        #region IDisposable

        public void Dispose()
        {
            _Viewport.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
