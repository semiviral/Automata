#region

using System;
using Automata.Core;
using Automata.Core.Systems;
using Automata.Singletons;
using Silk.NET.OpenGL;

#endregion

namespace Automata.Rendering
{
    public class RenderSystem : ComponentSystem
    {
        private readonly GL _GL;


        public RenderSystem()
        {
            HandledComponentTypes = new[]
            {
                typeof(Camera),
                typeof(PackedMesh)
            };

            GLAPI.Validate();
            _GL = GLAPI.Instance.GL;
        }

        public override unsafe void Update(EntityManager entityManager, TimeSpan delta)
        {
            _GL.ClearColor(0f, 0f, 0f, 1f);
            _GL.Clear((uint)ClearBufferMask.ColorBufferBit);

            foreach (Camera camera in entityManager.GetComponents<Camera>())
            {
                if (camera.Shader == null)
                {
                    continue;
                }

                camera.Shader.Use();

                foreach (PackedMesh packedMesh in entityManager.GetComponents<PackedMesh>())
                {
                    if (packedMesh.IndexesBuffer.Length == 0)
                    {
                        continue;
                    }

                    packedMesh.VertexArrayObject.Bind();
                    _GL.DrawElements(PrimitiveType.Triangles, packedMesh.IndexesBuffer.Length, DrawElementsType.UnsignedInt, null);

                    if (_GL.GetError() != GLEnum.NoError)
                    {
                        throw new Exception();
                    }
                }
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
