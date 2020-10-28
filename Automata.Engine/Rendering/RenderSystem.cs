#region

using System;
using System.Numerics;
using Automata.Engine.Components;
using Automata.Engine.Entities;
using Automata.Engine.Numerics;
using Automata.Engine.Numerics.Shapes;
using Automata.Engine.Rendering.GLFW;
using Automata.Engine.Rendering.Meshes;
using Automata.Engine.Rendering.OpenGL;
using Automata.Engine.Rendering.OpenGL.Textures;
using Automata.Engine.Systems;
using Serilog;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Plane = Automata.Engine.Numerics.Shapes.Plane;

#endregion


namespace Automata.Engine.Rendering
{
    public class RenderSystem : ComponentSystem
    {
        private const bool _ENABLE_BACK_FACE_CULLING = true;

        private readonly GL _GL;
        private readonly Texture _Texture;

        private float _NewAspectRatio;

        public RenderSystem()
        {
            GameWindowResized(null!, AutomataWindow.Instance.Size);
            AutomataWindow.Instance.Resized += GameWindowResized;

            _GL = GLAPI.Instance.GL;
            _GL.ClearColor(0.2f, 0.2f, 0.2f, 1f);
            _GL.Enable(GLEnum.DepthTest);

            if (_ENABLE_BACK_FACE_CULLING)
            {
                // enable and configure face culling
                _GL.FrontFace(FrontFaceDirection.Ccw);
                _GL.CullFace(CullFaceMode.Back);
                _GL.Enable(GLEnum.CullFace);
            }

            Image<Rgba32> image = Image.Load<Rgba32>("Resources/Textures/BlankMagenta.png");
            image.Mutate(img => img.Flip(FlipMode.Vertical));

            Texture2DArray<Rgba32> texture = new Texture2DArray<Rgba32>(new Vector3i(16, 16, 1), Texture.WrapMode.Repeat, Texture.FilterMode.Point);
            texture.SetPixels(Vector3i.Zero, new Vector2i(image.Width, image.Height), ref image.GetPixelRowSpan(0)[0]);

            _Texture = // texture;

            Texture2D<Rgba32>.LoadFromFile("Resources/Textures/BlankMagenta.png", Texture.WrapMode.Repeat, Texture.FilterMode.Point, false);
            // TextureRegistry.Instance.GetTexture<Texture2DArray<Rgba32>>("blocks") ?? throw new NullReferenceException();
        }

        [HandlesComponents(DistinctionStrategy.Any, typeof(Camera), typeof(RenderMesh))]
        public override unsafe void Update(EntityManager entityManager, TimeSpan delta)
        {
            try
            {
                _GL.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

                Vector4 viewport = new Vector4(0f, 0f, AutomataWindow.Instance.Size.X, AutomataWindow.Instance.Size.Y);
                Span<Plane> planes = stackalloc Plane[Frustum.TOTAL_PLANES];

                foreach (IEntity cameraEntity in entityManager.GetEntitiesWithComponents<Camera>())
                {
                    Camera camera = cameraEntity.GetComponent<Camera>();

                    if ((cameraEntity.TryGetComponent(out Scale? cameraScale) && cameraScale.Changed)
                        | (cameraEntity.TryGetComponent(out Translation? cameraTranslation) && cameraTranslation.Changed)
                        | (cameraEntity.TryGetComponent(out Rotation? cameraRotation) && cameraRotation.Changed))
                    {
                        camera.View = Matrix4x4.Identity;
                        camera.View *= Matrix4x4.CreateScale(cameraScale?.Value ?? Scale.DEFAULT);
                        camera.View *= Matrix4x4.CreateFromQuaternion(cameraRotation?.Value ?? Quaternion.Identity);
                        camera.View *= Matrix4x4.CreateTranslation(cameraTranslation?.Value ?? Vector3.Zero);

                        Matrix4x4.Invert(camera.View, out Matrix4x4 inverted);
                        camera.View = inverted;
                    }

                    if (_NewAspectRatio > 0f) camera.CalculateProjection(90f, _NewAspectRatio, 0.1f, 1000f);

                    Matrix4x4 viewProjection = camera.View * camera.Projection;
                    RenderShader? currentShader = null;

                    foreach (IEntity objectEntity in entityManager.GetEntitiesWithComponents<RenderMesh>())
                    {
                        RenderMesh renderMesh = objectEntity.GetComponent<RenderMesh>();

                        if (((objectEntity.TryGetComponent(out Scale? modelScale) && modelScale.Changed)
                             | (objectEntity.TryGetComponent(out Rotation? modelRotation) && modelRotation.Changed)
                             | (objectEntity.TryGetComponent(out Translation? modelTranslation) && modelTranslation.Changed))
                            || renderMesh.Changed)
                        {
                            renderMesh.Model = Matrix4x4.Identity;
                            renderMesh.Model *= Matrix4x4.CreateTranslation(modelTranslation?.Value ?? Vector3.Zero);
                            renderMesh.Model *= Matrix4x4.CreateFromQuaternion(modelRotation?.Value ?? Quaternion.Identity);
                            renderMesh.Model *= Matrix4x4.CreateScale(modelScale?.Value ?? Scale.DEFAULT);
                        }

                        Matrix4x4 modelViewProjection = renderMesh.Model * viewProjection;

                        if (!renderMesh.ShouldRender // check if should render at all
                            || !objectEntity.TryGetComponent(out RenderShader? renderShader) // if no RenderShader component, don't try to render
                            // check if occluded by frustum
                            || (objectEntity.TryGetComponent(out Bounds? bounds) && CheckClipFrustumOcclude(bounds, planes, modelViewProjection))) continue;

                        if (currentShader is null || (renderShader.Value.ID != currentShader.Value.ID))
                        {
                            renderShader.Value.Use();
                            currentShader = renderShader;
                        }

                        if (renderShader.Value.HasAutomataUniforms)
                        {
                            if (Matrix4x4.Invert(renderMesh.Model, out Matrix4x4 modelInverted))
                                renderShader.Value.TrySetUniform(Shader.RESERVED_UNIFORM_NAME_MATRIX_OBJECT, modelInverted);

                            renderShader.Value.TrySetUniform(Shader.RESERVED_UNIFORM_NAME_MATRIX_WORLD, renderMesh.Model);
                            renderShader.Value.TrySetUniform(Shader.RESERVED_UNIFORM_NAME_MATRIX_MVP, modelViewProjection);

                            renderShader.Value.TrySetUniform(Shader.RESERVED_UNIFORM_NAME_VEC3_CAMERA_WORLD_POSITION,
                                cameraTranslation?.Value ?? Vector3.Zero);

                            renderShader.Value.TrySetUniform(Shader.RESERVED_UNIFORM_NAME_VEC4_CAMERA_PROJECTION_PARAMS, camera.ProjectionParameters);
                            renderShader.Value.TrySetUniform(Shader.RESERVED_UNIFORM_NAME_VEC4_VIEWPORT, viewport);
                        }

                        renderMesh.Mesh!.Bind();
                        _Texture.Bind(TextureUnit.Texture0);
                        renderShader.Value.TrySetUniform(Shader.RESERVED_UNIFROM_NAME_SAMPLER2DARRAY_BLOCKS, 0);

                        _GL.DrawElements(PrimitiveType.Triangles, renderMesh.Mesh!.IndexesLength, DrawElementsType.UnsignedInt, null);
                    }
                }

                _GL.BindVertexArray(0);
                _NewAspectRatio = 0f;

                GLAPI.CheckForErrorsAndThrow(true);
            }
            catch (Exception exception)
            {
                Log.Error($"({nameof(RenderSystem)}) Error: {exception}\r\n{exception.StackTrace}");
            }
        }

        private static bool CheckClipFrustumOcclude(Bounds bounds, Span<Plane> planes, Matrix4x4 mvp)
        {
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

        private void GameWindowResized(object sender, Vector2i newSize) => _NewAspectRatio = (float)newSize.X / (float)newSize.Y;

        protected override void DisposeInternal()
        {
            base.DisposeInternal();
            _Texture.Dispose();
        }
    }
}
