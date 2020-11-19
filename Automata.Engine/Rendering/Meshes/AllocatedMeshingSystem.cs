using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Automata.Engine.Collections;
using Automata.Engine.Rendering.OpenGL;
using Automata.Engine.Rendering.OpenGL.Buffers;
using Automata.Engine.Rendering.OpenGL.Shaders;
using Automata.Engine.Rendering.OpenGL.Textures;
using Serilog;

namespace Automata.Engine.Rendering.Meshes
{
    public class AllocatedMeshingSystem<TIndex, TVertex> : ComponentSystem
        where TIndex : unmanaged, IEquatable<TIndex>
        where TVertex : unmanaged, IEquatable<TVertex>
    {
        private MultiDrawIndirectMesh<TIndex, TVertex>? _MultiDrawIndirectMesh;
        private Material? _MultiDrawIndirectMeshMaterial;

        public override void Registered(EntityManager entityManager)
        {
            _MultiDrawIndirectMeshMaterial =
                new Material(ProgramRegistry.Instance.Load("Resources/Shaders/PackedVertex.glsl", "Resources/Shaders/DefaultFragment.glsl"));

            _MultiDrawIndirectMesh = new MultiDrawIndirectMesh<TIndex, TVertex>(GLAPI.Instance.GL, 750_000_000, 500_000_000);

            entityManager.CreateEntity(
                _MultiDrawIndirectMeshMaterial,
                new RenderMesh
                {
                    Mesh = _MultiDrawIndirectMesh
                });
        }

        public override ValueTask Update(EntityManager entityManager, TimeSpan delta)
        {
            bool recreateCommandBuffer = false;

            foreach ((IEntity entity, AllocatedMeshData<TIndex, TVertex> mesh) in entityManager.GetEntitiesWithComponents<AllocatedMeshData<TIndex, TVertex>>())
            {
                if (TryAllocateMesh(entityManager, entity, mesh.Data))
                {
                    ConfigureMaterial(entityManager, entity);
                }

                entityManager.RemoveComponent<AllocatedMeshData<TIndex, TVertex>>(entity);
                recreateCommandBuffer = true;
            }

            if (recreateCommandBuffer)
            {
                using NonAllocatingList<MultiDrawIndirectAllocation<TIndex, TVertex>> allocations =
                    new NonAllocatingList<MultiDrawIndirectAllocation<TIndex, TVertex>>();

                using NonAllocatingList<Matrix4x4> models = new NonAllocatingList<Matrix4x4>();

                foreach ((IEntity entity, MultiDrawIndirectAllocation<TIndex, TVertex> allocation) in
                    entityManager.GetEntitiesWithComponents<MultiDrawIndirectAllocation<TIndex, TVertex>>())
                {
                    allocations.Add(allocation);
                    models.Add(entity.Find<RenderModel>()?.Model ?? Matrix4x4.Identity);
                }

                GenerateDrawElementsIndirectCommands(allocations.Segment);
                _MultiDrawIndirectMesh!.AllocateModelsData(models.Segment);
            }

            return ValueTask.CompletedTask;
        }

        public void SetTexture(string key, Texture texture)
        {
            if (_MultiDrawIndirectMeshMaterial is null)
            {
                ThrowHelper.ThrowNullReferenceException(nameof(_MultiDrawIndirectMeshMaterial));
            }

            if (!_MultiDrawIndirectMeshMaterial!.Textures.ContainsKey(key))
            {
                _MultiDrawIndirectMeshMaterial.Textures.Add(key, texture);
            }
            else
            {
                _MultiDrawIndirectMeshMaterial.Textures[key] = texture;
            }
        }

        public void AllocateVertexAttributes(bool replace, bool finalize, params IVertexAttribute[] attributes)
        {
            if (_MultiDrawIndirectMesh is null)
            {
                ThrowHelper.ThrowNullReferenceException(nameof(_MultiDrawIndirectMesh));
            }

            _MultiDrawIndirectMesh!.AllocateVertexAttributes(replace, attributes);

            if (finalize)
            {
                _MultiDrawIndirectMesh!.FinalizeVertexArrayObject();
            }
        }

        public void FinalizeVertexArrayObject() => _MultiDrawIndirectMesh!.FinalizeVertexArrayObject();


        #region Data Processing

        private unsafe void GenerateDrawElementsIndirectCommands(Span<MultiDrawIndirectAllocation<TIndex, TVertex>> allocations)
        {
            if (_MultiDrawIndirectMesh is null)
            {
                ThrowHelper.ThrowNullReferenceException(nameof(_MultiDrawIndirectMesh));
            }

            Span<DrawElementsIndirectCommand> commands = stackalloc DrawElementsIndirectCommand[allocations.Length];

            for (uint index = 0; index < allocations.Length; index++)
            {
                MultiDrawIndirectAllocation<TIndex, TVertex> multiDrawIndirectAllocation = allocations[(int)index];

                if (multiDrawIndirectAllocation.Allocation is null)
                {
                    throw new NullReferenceException("Allocation should not be null at this point.");
                }

                commands[(int)index] = new DrawElementsIndirectCommand(multiDrawIndirectAllocation.Allocation.VertexArrayMemory.Count, 1u,
                    (uint)(multiDrawIndirectAllocation.Allocation.IndexesArrayMemory.Index / (nuint)sizeof(TIndex)),
                    (uint)(multiDrawIndirectAllocation.Allocation.VertexArrayMemory.Index / (nuint)sizeof(TVertex)), index);
            }

            _MultiDrawIndirectMesh!.AllocateDrawElementsIndirectCommands(commands);

            Log.Verbose(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(AllocatedMeshingSystem<TIndex, TVertex>),
                $"Allocated {commands.Length} {nameof(DrawElementsIndirectCommand)}"));
        }

        private unsafe bool TryAllocateMesh(EntityManager entityManager, IEntity entity, NonAllocatingQuadsMeshData<TIndex, TVertex> pendingData)
        {
            if (_MultiDrawIndirectMesh is null)
            {
                throw new NullReferenceException("Mesh is null!");
            }
            else if (pendingData.IsEmpty)
            {
                return false;
            }

            // if the entity doesn't have the required component, make sure we add it
            if (!entity.TryFind(out MultiDrawIndirectAllocation<TIndex, TVertex>? drawIndirectAllocation))
            {
                drawIndirectAllocation = entityManager.RegisterComponent<MultiDrawIndirectAllocation<TIndex, TVertex>>(entity);
            }

            // we make sure to dispose the old allocation to free the memory in the pool
            drawIndirectAllocation.Allocation?.Dispose();
            _MultiDrawIndirectMesh.WaitForBufferSync();

            // todo copy all of this data on a separate thread, with sync obj maybe

            BufferArrayMemory<TIndex> indexArrayMemory = _MultiDrawIndirectMesh.RentIndexBufferArrayMemory((nuint)sizeof(TIndex),
                MemoryMarshal.Cast<QuadIndexes<TIndex>, TIndex>(pendingData.Indexes.Segment));

            BufferArrayMemory<TVertex> vertexArrayMemory = _MultiDrawIndirectMesh.RentVertexBufferArrayMemory((nuint)sizeof(TVertex),
                MemoryMarshal.Cast<QuadVertexes<TVertex>, TVertex>(pendingData.Vertexes.Segment));

            drawIndirectAllocation.Allocation = new MeshArrayMemory<TIndex, TVertex>(indexArrayMemory, vertexArrayMemory);

            Log.Verbose(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(AllocatedMeshingSystem<TIndex, TVertex>),
                $"Allocated new {nameof(MultiDrawIndirectAllocation<TIndex, TVertex>)}: {indexArrayMemory.MemoryOwner.Memory.Length} indexes, {vertexArrayMemory.MemoryOwner.Memory.Length} vertexes"));

            return true;
        }

        private static void ConfigureMaterial(EntityManager entityManager, IEntity entity)
        {
            ProgramPipeline programPipeline = ProgramRegistry.Instance.Load("Resources/Shaders/PackedVertex.glsl", "Resources/Shaders/DefaultFragment.glsl");

            if (entity.TryFind(out Material? material))
            {
                if (material.Pipeline.Handle != programPipeline.Handle)
                {
                    material.Pipeline = programPipeline;
                }
            }
            else
            {
                entityManager.RegisterComponent(entity, new Material(programPipeline));
            }
        }

        #endregion
    }
}
