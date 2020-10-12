#region

using System;
using System.Numerics;
using Automata.Entity;
using Automata.Rendering.GLFW;
using Automata.Rendering.Meshes;
using Automata.Rendering.OpenGL;
using Automata.System;
using Serilog;
using Silk.NET.OpenGL;

#endregion

namespace Automata.Rendering
{
    public class RenderSystem : ComponentSystem
    {
        private const bool _ENABLE_BACK_FACE_CULLING = true;

        private readonly GL _GL;

        public RenderSystem()
        {
            HandledComponents = new ComponentTypes(typeof(Translation), typeof(Camera), typeof(RenderMesh));

            _GL = GLAPI.Instance.GL;

            _GL.ClearColor(0.2f, 0.2f, 0.2f, 1f);

            // enable depth testing
            _GL.Enable(GLEnum.DepthTest);

            if (_ENABLE_BACK_FACE_CULLING)
            {
                // enable and configure face culling
                _GL.Enable(GLEnum.CullFace);
                _GL.FrontFace(FrontFaceDirection.Ccw);
                _GL.CullFace(CullFaceMode.Back);
            }
        }

        public override unsafe void Update(EntityManager entityManager, TimeSpan delta)
        {
            try
            {
                _GL.Clear((uint)ClearBufferMask.ColorBufferBit);
                _GL.Clear((uint)ClearBufferMask.DepthBufferBit);

                Vector4 viewport = new Vector4(0f, 0f, AutomataWindow.Instance.Size.X, AutomataWindow.Instance.Size.Y);

                foreach ((Translation cameraTranslation, Camera camera, RenderShader renderShader) in
                    entityManager.GetComponents<Translation, Camera, RenderShader>())
                {

                    renderShader.Shader.Use();
                    renderShader.Shader.TrySetUniform(Shader.RESERVED_UNIFORM_NAME_VEC3_CAMERA_WORLD_POSITION, cameraTranslation.Value);
                    renderShader.Shader.TrySetUniform(Shader.RESERVED_UNIFORM_NAME_VEC4_CAMERA_PROJECTION_PARAMS, camera.ProjectionParameters);
                    renderShader.Shader.TrySetUniform(Shader.RESERVED_UNIFORM_NAME_VEC4_VIEWPORT, viewport);

                    RenderMesh? currentRenderMesh = null;

                    foreach (IEntity entity in entityManager.GetEntitiesWithComponents<RenderMesh>())
                    {
                        RenderMesh nextRenderMesh = entity.GetComponent<RenderMesh>();

                        if ((nextRenderMesh.Mesh == null) || !nextRenderMesh.Mesh.Visible || (nextRenderMesh.Mesh.IndexesLength == 0))
                        {
                            continue;
                        }

                        if ((currentRenderMesh == null) || (currentRenderMesh.MeshID != nextRenderMesh.MeshID))
                        {
                            currentRenderMesh = nextRenderMesh;
                            currentRenderMesh.Mesh.BindVertexArrayObject();
                        }

                        if (renderShader.Shader.HasAutomataUniforms)
                        {
                            Matrix4x4 model = Matrix4x4.Identity;

                            if (entity.TryGetComponent(out Scale modelScale))
                            {
                                model *= Matrix4x4.CreateScale(modelScale.Value);
                            }

                            if (entity.TryGetComponent(out Rotation modelRotation))
                            {
                                model *= Matrix4x4.CreateFromQuaternion(modelRotation.Value);
                            }

                            if (entity.TryGetComponent(out Translation modelTranslation))
                            {
                                model *= Matrix4x4.CreateTranslation(modelTranslation.Value);
                            }

                            Matrix4x4 modelView = model * camera.View;
                            Matrix4x4 modelViewProjection = modelView * camera.Projection;

                            renderShader.Shader.TrySetUniform(Shader.RESERVED_UNIFORM_NAME_MATRIX_MV, modelView);
                            renderShader.Shader.TrySetUniform(Shader.RESERVED_UNIFORM_NAME_MATRIX_MVP, modelViewProjection);
                            renderShader.Shader.TrySetUniform(Shader.RESERVED_UNIFORM_NAME_MATRIX_WORLD, model);

                            if (Matrix4x4.Invert(model, out Matrix4x4 modelInverted))
                            {
                                renderShader.Shader.TrySetUniform(Shader.RESERVED_UNIFORM_NAME_MATRIX_OBJECT, modelInverted);
                            }
                        }

                        _GL.DrawElements(PrimitiveType.Triangles, currentRenderMesh.Mesh?.IndexesLength ?? 0u, DrawElementsType.UnsignedInt, null);

#if DEBUG
                        if (_GL.GetError() != GLEnum.NoError)
                        {
                            throw new Exception();
                        }
#endif
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"({nameof(RenderSystem)}) Error: {ex.Message}\r\n{ex.StackTrace}");
            }
        }
    }
}
