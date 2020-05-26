#region

using System;
using System.Numerics;
using Automata.Rendering.OpenGL;
using Automata.Worlds;
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
            HandledComponentTypes = new[]
            {
                typeof(Translation),
                typeof(Camera),
                typeof(RenderMesh)
            };

            _GL = GLAPI.Instance.GL;

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
                _GL.ClearColor(0f, 0f, 0f, 1f);
                _GL.Clear((uint)ClearBufferMask.ColorBufferBit);
                _GL.Clear((uint)ClearBufferMask.DepthBufferBit);

                foreach ((Translation translation, Camera camera) in entityManager.GetComponents<Translation, Camera>())
                {
                    if (camera.Shader == null)
                    {
                        continue;
                    }

                    camera.Shader.Use();

                    foreach (IEntity entity in entityManager.GetEntitiesWithComponents<RenderMesh>())
                    {
                        RenderMesh renderMesh = entity.GetComponent<RenderMesh>();

                        if ((renderMesh.Mesh == null) || (renderMesh.Mesh.IndexesCount == 0))
                        {
                            continue;
                        }

                        Matrix4x4 model = Matrix4x4.Identity;

                        if (entity.TryGetComponent(out Scale scale))
                        {
                            model *= Matrix4x4.CreateScale(scale.Value);
                        }

                        if (entity.TryGetComponent(out Rotation rotation))
                        {
                            model *= Matrix4x4.CreateFromQuaternion(rotation.Value);
                        }

                        if (entity.TryGetComponent(out Translation modelTranslation))
                        {
                            model *= Matrix4x4.CreateTranslation(modelTranslation.Value);
                        }

                        camera.Shader.TrySetUniform(Shader.RESERVED_UNIFORM_NAME_MATRIX_MVP, model * camera.View * camera.Projection);
                        camera.Shader.TrySetUniform(Shader.RESERVED_UNIFORM_NAME_MATRIX_WORLD, model);

                        if (Matrix4x4.Invert(model, out Matrix4x4 modelInverted))
                        {
                            camera.Shader.TrySetUniform(Shader.RESERVED_UNIFORM_NAME_MATRIX_OBJECT, modelInverted);
                        }

                        camera.Shader.TrySetUniform(Shader.RESERVED_UNIFORM_NAME_VEC3_CAMERA_WORLD_POSITION, translation.Value);
                        camera.Shader.TrySetUniform(Shader.RESERVED_UNIFORM_NAME_VEC4_CAMERA_PROJECTION_PARAMS, camera.ProjectionParameters);

                        renderMesh.Mesh.BindVertexArrayObject();
                        _GL.DrawElements(PrimitiveType.Triangles, renderMesh.Mesh.IndexesCount, DrawElementsType.UnsignedInt, null);

                        if (_GL.GetError() != GLEnum.NoError)
                        {
                            throw new Exception();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"({nameof(RenderSystem)}) Error: {ex.Message}");
            }
        }

        public override void Destroy(EntityManager entityManager)
        {
            // foreach (IMesh renderedMeshComponent in entityManager.GetComponents<IMesh>())
            // {
            //     renderedMeshComponent.Dispose();
            // }
        }
    }
}
