#region

using System;
using System.Numerics;
using Automata.Engine.Components;
using Automata.Engine.Entities;
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

        public RenderSystem()
        {
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

        [HandlesComponents(DistinctionStrategy.All, typeof(Translation), typeof(Camera), typeof(RenderMesh))]
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
                    renderShader.Value.Use();

                    foreach (IEntity entity in entityManager.GetEntitiesWithComponents<RenderMesh>())
                    {
                        RenderMesh renderMesh = entity.GetComponent<RenderMesh>();

                        if (renderMesh.Mesh is null
                            || !renderMesh.Mesh.Visible
                            || (renderMesh.Mesh.IndexesLength == 0))
                        {
                            continue;
                        }

                        renderMesh.Mesh.BindVertexArrayObject();

                        if (renderShader.Value.HasAutomataUniforms)
                        {
                            if ((entity.TryGetComponent(out Scale? modelScale) && modelScale.Changed)
                                | (entity.TryGetComponent(out Rotation? modelRotation) && modelRotation.Changed)
                                | (entity.TryGetComponent(out Translation? modelTranslation) && modelTranslation.Changed))
                            {
                                renderMesh.Model = Matrix4x4.Identity;
                                renderMesh.Model *= Matrix4x4.CreateScale(modelScale?.Value ?? Scale.DEFAULT);
                                renderMesh.Model *= Matrix4x4.CreateFromQuaternion(modelRotation?.Value ?? Quaternion.Identity);
                                renderMesh.Model *= Matrix4x4.CreateTranslation(modelTranslation?.Value ?? Vector3.Zero);
                            }

                            Matrix4x4.Invert(renderMesh.Model, out Matrix4x4 modelInverted);
                            Matrix4x4 modelView = renderMesh.Model * camera.View;
                            Matrix4x4 modelViewProjection = modelView * camera.Projection;

                            renderShader.Value.TrySetUniform(Shader.RESERVED_UNIFORM_NAME_MATRIX_WORLD, renderMesh.Model);
                            renderShader.Value.TrySetUniform(Shader.RESERVED_UNIFORM_NAME_MATRIX_MV, modelView);
                            renderShader.Value.TrySetUniform(Shader.RESERVED_UNIFORM_NAME_MATRIX_MVP, modelViewProjection);
                            renderShader.Value.TrySetUniform(Shader.RESERVED_UNIFORM_NAME_MATRIX_OBJECT, modelInverted);
                            renderShader.Value.TrySetUniform(Shader.RESERVED_UNIFORM_NAME_VEC3_CAMERA_WORLD_POSITION, cameraTranslation.Value);
                            renderShader.Value.TrySetUniform(Shader.RESERVED_UNIFORM_NAME_VEC4_CAMERA_PROJECTION_PARAMS, camera.ProjectionParameters);
                            renderShader.Value.TrySetUniform(Shader.RESERVED_UNIFORM_NAME_VEC4_VIEWPORT, viewport);
                        }

                        _GL.DrawElements(PrimitiveType.Triangles, renderMesh.Mesh?.IndexesLength ?? 0u, DrawElementsType.UnsignedInt, null);

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
