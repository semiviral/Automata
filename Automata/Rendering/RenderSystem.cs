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
        private const bool _ENABLE_BACK_FACE_CULLING = false;

        private readonly GL _GL;

        public RenderSystem()
        {
            HandledComponentTypes = new[]
            {
                typeof(Camera),
                typeof(PackedMesh)
            };

            _GL = GLAPI.Instance.GL;

            // enable depth testing
            _GL.Enable(GLEnum.DepthTest);

            if (_ENABLE_BACK_FACE_CULLING)
            {
                // enable and configure face culling
                _GL.Enable(GLEnum.CullFace);
                _GL.FrontFace(FrontFaceDirection.CW);
                _GL.CullFace(CullFaceMode.Back);
            }
        }

        public override void Registered() { }

        private void DrawFrame() { }

        public override unsafe void Update(EntityManager entityManager, TimeSpan delta)
        {
            try
            {
                _GL.ClearColor(0f, 0f, 0f, 1f);
                _GL.Clear((uint)ClearBufferMask.ColorBufferBit);
                _GL.Clear((uint)ClearBufferMask.DepthBufferBit);

                foreach (Camera camera in entityManager.GetComponents<Camera>())
                {
                    if (camera.Shader == null)
                    {
                        continue;
                    }

                    camera.Shader.Use();

                    foreach (IEntity entity in entityManager.GetEntitiesWithComponents<PackedMesh>())
                    {
                        PackedMesh packedMesh = entity.GetComponent<PackedMesh>();

                        if (packedMesh.IndexesBuffer.Length == 0)
                        {
                            continue;
                        }

                        Matrix4x4 model = Matrix4x4.Identity;

                        if (entity.TryGetComponent(out Rotation rotation))
                        {
                            model *= Matrix4x4.CreateFromQuaternion(rotation.Value);
                        }

                        if (entity.TryGetComponent(out Scale scale))
                        {
                            model *= Matrix4x4.CreateScale(scale.Value);
                        }

                        if (entity.TryGetComponent(out Translation translation))
                        {
                            model *= Matrix4x4.CreateTranslation(translation.Value);
                        }

                        camera.Shader.SetUniform("MVP_Matrix", model * camera.View * camera.Projection);

                        packedMesh.VertexArrayObject.Bind();
                        _GL.DrawElements(PrimitiveType.Triangles, packedMesh.IndexesBuffer.Length, DrawElementsType.UnsignedInt, null);

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
