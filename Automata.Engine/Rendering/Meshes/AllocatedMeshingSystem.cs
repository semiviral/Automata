using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Automata.Engine.Collections;
using Automata.Engine.Diagnostics;
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
                ProcessMeshData(entityManager, entity, mesh.Data);
                entityManager.RemoveComponent<AllocatedMeshData<TIndex, TVertex>>(entity);
                recreateCommandBuffer = true;
            }

            if (recreateCommandBuffer)
            {
                using NonAllocatingList<MultiDrawIndirectAllocation<TIndex, TVertex>> allocations = new NonAllocatingList<MultiDrawIndirectAllocation<TIndex, TVertex>>();
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
            if (_MultiDrawIndirectMeshMaterial is null) ThrowHelper.ThrowNullReferenceException(nameof(_MultiDrawIndirectMeshMaterial));

            if (!_MultiDrawIndirectMeshMaterial!.Textures.ContainsKey(key)) _MultiDrawIndirectMeshMaterial.Textures.Add(key, texture);
            else _MultiDrawIndirectMeshMaterial.Textures[key] = texture;
        }

        public void AllocateVertexAttributes(bool replace, bool finalize, params IVertexAttribute[] attributes)
        {
            if (_MultiDrawIndirectMesh is null) ThrowHelper.ThrowNullReferenceException(nameof(_MultiDrawIndirectMesh));

            _MultiDrawIndirectMesh!.AllocateVertexAttributes(replace, attributes);
            if (finalize) _MultiDrawIndirectMesh!.FinalizeVertexArrayObject();
        }


        #region Data Processing

        private unsafe void GenerateDrawElementsIndirectCommands(Span<MultiDrawIndirectAllocation<TIndex, TVertex>> allocations)
        {
            if (_MultiDrawIndirectMesh is null) ThrowHelper.ThrowNullReferenceException(nameof(_MultiDrawIndirectMesh));

            Span<DrawElementsIndirectCommand> commands = stackalloc DrawElementsIndirectCommand[allocations.Length];

            for (uint index = 0; index < allocations.Length; index++)
            {
                MultiDrawIndirectAllocation<TIndex, TVertex> multiDrawIndirectAllocation = allocations[(int)index];
                if (multiDrawIndirectAllocation.Allocation is null) throw new NullReferenceException("Allocation should not be null at this point.");

                nuint indexesStart = multiDrawIndirectAllocation.Allocation.IndexesArrayMemory.Index / (nuint)sizeof(TIndex);
                nuint vertexesStart = multiDrawIndirectAllocation.Allocation.VertexArrayMemory.Index;

                commands[(int)index] = new DrawElementsIndirectCommand(multiDrawIndirectAllocation.Allocation.VertexArrayMemory.Count, 1u,
                    (uint)indexesStart, (uint)vertexesStart, index);
            }

            _MultiDrawIndirectMesh!.AllocateDrawElementsIndirectCommands(commands);

            Log.Verbose(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(AllocatedMeshingSystem<TIndex, TVertex>),
                $"Allocated {commands.Length} {nameof(DrawElementsIndirectCommand)}"));
        }

        private void ProcessMeshData(EntityManager entityManager, IEntity entity, NonAllocatingQuadsMeshData<TIndex, TVertex> pendingData)
        {
            Stopwatch stopwatch = DiagnosticsPool.Stopwatches.Rent();
            stopwatch.Restart();

            if (ApplyMeshMultiDraw(entityManager, entity, pendingData)) ConfigureMaterial(entityManager, entity);

            DiagnosticsPool.Stopwatches.Return(stopwatch);
        }

        private unsafe bool ApplyMeshMultiDraw(EntityManager entityManager, IEntity entity, NonAllocatingQuadsMeshData<TIndex, TVertex> pendingData)
        {
            if (_MultiDrawIndirectMesh is null) throw new NullReferenceException("Mesh is null!");
            else if (pendingData.IsEmpty) return false;

            // if the entity doesn't have the required component, make sure we add it
            if (!entity.TryFind(out MultiDrawIndirectAllocation<TIndex, TVertex>? drawIndirectAllocation))
                drawIndirectAllocation = entityManager.RegisterComponent<MultiDrawIndirectAllocation<TIndex, TVertex>>(entity);

            // we make sure to dispose the old allocation to
            // free the memory in the pool
            drawIndirectAllocation.Allocation?.Dispose();
            _MultiDrawIndirectMesh.WaitForBufferSync();

            BufferArrayMemory<TIndex> indexArrayMemory = _MultiDrawIndirectMesh.RentIndexBufferArrayMemory((nuint)sizeof(TIndex),
                MemoryMarshal.Cast<QuadIndexes<TIndex>, TIndex>(pendingData.Indexes.Segment));

            BufferArrayMemory<TVertex> vertexArrayMemory = _MultiDrawIndirectMesh.RentVertexBufferArrayMemory(0u,
                MemoryMarshal.Cast<QuadVertexes<TVertex>, TVertex>(pendingData.Vertexes.Segment));

            drawIndirectAllocation.Allocation = new AllocationWrapper<TIndex, TVertex>(indexArrayMemory, vertexArrayMemory);

            Log.Verbose(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(AllocatedMeshingSystem<TIndex, TVertex>),
                $"Allocated new {nameof(MultiDrawIndirectAllocation<TIndex, TVertex>)}: indexes ({pendingData.Indexes.Count * 6}, {indexArrayMemory.MemoryOwner.Memory.Length} bytes), vertexes ({pendingData.Vertexes.Count * 4}, {vertexArrayMemory.MemoryOwner.Memory.Length} bytes)"));

            return true;
        }

        private static void ConfigureMaterial(EntityManager entityManager, IEntity entity)
        {
            ProgramPipeline programPipeline = ProgramRegistry.Instance.Load("Resources/Shaders/PackedVertex.glsl", "Resources/Shaders/DefaultFragment.glsl");

            if (entity.TryFind(out Material? material))
            {
                if (material.Pipeline.Handle != programPipeline.Handle) material.Pipeline = programPipeline;
            }
            else entityManager.RegisterComponent(entity, new Material(programPipeline));
        }

        #endregion
    }
}
