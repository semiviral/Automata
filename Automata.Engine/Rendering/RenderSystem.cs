#region

using System;
using System.Numerics;
using Automata.Engine.Components;
using Automata.Engine.Entities;
using Automata.Engine.Numerics;
using Automata.Engine.Rendering.GLFW;
using Automata.Engine.Rendering.Meshes;
using Automata.Engine.Rendering.OpenGL;
using Automata.Engine.Systems;
using Serilog;
using Silk.NET.OpenGL;

#endregion


namespace Automata.Engine.Rendering
{
    public class RenderSystem : ComponentSystem
    {
        private const bool _ENABLE_BACK_FACE_CULLING = true;

        private readonly GL _GL;

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
        }

        [HandlesComponents(DistinctionStrategy.Any, typeof(Camera), typeof(RenderMesh))]
        public override unsafe void Update(EntityManager entityManager, TimeSpan delta)
        {
            try
            {
                _GL.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

                Vector4 viewport = new Vector4(0f, 0f, AutomataWindow.Instance.Size.X, AutomataWindow.Instance.Size.Y);

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

                    if (_NewAspectRatio > 0f) camera.CalculateProjection(_NewAspectRatio);

                    foreach (IEntity objectEntity in entityManager.GetEntitiesWithComponents<RenderMesh>())
                    {
                        RenderMesh renderMesh = objectEntity.GetComponent<RenderMesh>();

                        if (!renderMesh.ShouldRender || !objectEntity.TryGetComponent(out RenderShader? renderShader)) continue;

                        renderShader.Value.Use();
                        renderMesh.Mesh!.BindVertexArrayObject();

                        if (renderShader.Value.HasAutomataUniforms)
                        {
                            if (renderMesh.Changed
                                | (objectEntity.TryGetComponent(out Scale? modelScale) && modelScale.Changed)
                                | (objectEntity.TryGetComponent(out Rotation? modelRotation) && modelRotation.Changed)
                                | (objectEntity.TryGetComponent(out Translation? modelTranslation) && modelTranslation.Changed))
                            {
                                renderMesh.Model = Matrix4x4.Identity;
                                renderMesh.Model *= Matrix4x4.CreateTranslation(modelTranslation?.Value ?? Vector3.Zero);
                                renderMesh.Model *= Matrix4x4.CreateFromQuaternion(modelRotation?.Value ?? Quaternion.Identity);
                                renderMesh.Model *= Matrix4x4.CreateScale(modelScale?.Value ?? Scale.DEFAULT);
                            }

                            Matrix4x4.Invert(renderMesh.Model, out Matrix4x4 modelInverted);
                            Matrix4x4 modelView = renderMesh.Model * camera.View;
                            Matrix4x4 modelViewProjection = modelView * camera.Projection;

                            renderShader.Value.TrySetUniform(Shader.RESERVED_UNIFORM_NAME_MATRIX_WORLD, renderMesh.Model);
                            renderShader.Value.TrySetUniform(Shader.RESERVED_UNIFORM_NAME_MATRIX_MV, modelView);
                            renderShader.Value.TrySetUniform(Shader.RESERVED_UNIFORM_NAME_MATRIX_MVP, modelViewProjection);
                            renderShader.Value.TrySetUniform(Shader.RESERVED_UNIFORM_NAME_MATRIX_OBJECT, modelInverted);

                            renderShader.Value.TrySetUniform(Shader.RESERVED_UNIFORM_NAME_VEC3_CAMERA_WORLD_POSITION,
                                cameraTranslation?.Value ?? Vector3.Zero);

                            renderShader.Value.TrySetUniform(Shader.RESERVED_UNIFORM_NAME_VEC4_CAMERA_PROJECTION_PARAMS, camera.ProjectionParameters);
                            renderShader.Value.TrySetUniform(Shader.RESERVED_UNIFORM_NAME_VEC4_VIEWPORT, viewport);
                        }

                        _GL.DrawElements(PrimitiveType.Triangles, renderMesh.Mesh.IndexesLength, DrawElementsType.UnsignedInt, null);

                        CheckForGLErrorsAndThrow();
                    }
                }

                _NewAspectRatio = 0f;
            }
            catch (Exception ex)
            {
                Log.Error($"({nameof(RenderSystem)}) Error: {ex}\r\n{ex.StackTrace}");
            }
        }

        private void CheckForGLErrorsAndThrow()
        {
            GLEnum glError = _GL.GetError();

            switch (glError)
            {
                case GLEnum.NoError: break;
                default: throw new OpenGLException(glError);
            }
        }

        private void GameWindowResized(object sender, Vector2i newSize) { _NewAspectRatio = (float)newSize.X / (float)newSize.Y; }
    }
}
