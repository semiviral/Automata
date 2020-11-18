using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Automata.Engine.Diagnostics;
using Automata.Engine.Entities;
using Automata.Engine.Rendering.OpenGL;
using Automata.Engine.Rendering.OpenGL.Buffers;
using Automata.Engine.Rendering.OpenGL.Shaders;
using Automata.Engine.Rendering.OpenGL.Textures;
using Automata.Engine.Systems;
using Microsoft.Toolkit.HighPerformance.Extensions;

namespace Automata.Engine.Rendering.Meshes
{
    public class AllocatedMeshingSystem<TIndex, TVertex> : ComponentSystem
        where TIndex : unmanaged, IEquatable<TIndex>
        where TVertex : unmanaged, IEquatable<TVertex>
    {
        private MultiDrawIndirectMesh<TIndex, TVertex>? _MultiDrawIndirectMesh;
        private Material? _MultiDrawIndirectMeshMaterial;

        public void SetTexture(string key, Texture texture)
        {
            if (_MultiDrawIndirectMeshMaterial is null) ThrowHelper.ThrowNullReferenceException(nameof(_MultiDrawIndirectMeshMaterial));

            if (!_MultiDrawIndirectMeshMaterial!.Textures.ContainsKey(key)) _MultiDrawIndirectMeshMaterial.Textures.Add(key, texture);
            else _MultiDrawIndirectMeshMaterial.Textures[key] = texture;
        }


        #region ComponentSystem

        public override void Registered(EntityManager entityManager)
        {
            _MultiDrawIndirectMeshMaterial =
                new Material(ProgramRegistry.Instance.Load("Resources/Shaders/PackedVertex.glsl", "Resources/Shaders/DefaultFragment.glsl"));

            _MultiDrawIndirectMesh = new MultiDrawIndirectMesh<TIndex, TVertex>(GLAPI.Instance.GL, 750_000_000, 500_000_000);

            _MultiDrawIndirectMesh.AllocateVertexAttributes(true,
                new VertexAttribute<int>(0u, 1u, 0u),
                new VertexAttribute<int>(1u, 1u, 4u)

                //new VertexAttribute<float>(2u + 0u, 4u, 0u, 0u, 1u)
            );

            _MultiDrawIndirectMesh.FinalizeVertexArrayObject(0);

            //material.Textures.Add(TextureAtlas.Instance.Blocks!);

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

            if (recreateCommandBuffer) GenerateDrawElementsIndirectCommands(entityManager.GetComponents<DrawIndirectAllocation<TIndex, TVertex>>());

            return ValueTask.CompletedTask;
        }

        #endregion


        #region Data Processing

        private unsafe void GenerateDrawElementsIndirectCommands(IEnumerable<DrawIndirectAllocation<TIndex, TVertex>> drawIndirectAllocations)
        {
            if (_MultiDrawIndirectMesh is null) ThrowHelper.ThrowNullReferenceException(nameof(_MultiDrawIndirectMesh));

            DrawIndirectAllocation<TIndex, TVertex>[] allocations = drawIndirectAllocations.ToArray();
            Span<DrawElementsIndirectCommand> commands = stackalloc DrawElementsIndirectCommand[allocations.Length];

            for (int index = 0; index < allocations.Length; index++)
            {
                DrawIndirectAllocation<TIndex, TVertex> drawIndirectAllocation = allocations[index];
                if (drawIndirectAllocation.Allocation is null) ThrowHelper.ThrowNullReferenceException(nameof(drawIndirectAllocation.Allocation));

                nuint indexesStart = drawIndirectAllocation.Allocation!.IndexesArrayMemory.Index / (nuint)sizeof(TIndex);
                nuint vertexesStart = drawIndirectAllocation.Allocation!.VertexArrayMemory.Index;

                commands[index] = new DrawElementsIndirectCommand(drawIndirectAllocation.Allocation!.VertexArrayMemory.Count, 1u,
                    (uint)indexesStart, (uint)vertexesStart, (uint)index);
            }

            _MultiDrawIndirectMesh!.AllocateDrawElementsIndirectCommands(commands);
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

            if (!entity.TryFind(out DrawIndirectAllocation<TIndex, TVertex>? drawIndirectAllocation))
                drawIndirectAllocation = entityManager.RegisterComponent<DrawIndirectAllocation<TIndex, TVertex>>(entity);

            drawIndirectAllocation.Allocation?.Dispose();
            _MultiDrawIndirectMesh.WaitForBufferFreeSync();

            drawIndirectAllocation.Allocation = new AllocationWrapper<TIndex, TVertex>(
                new BufferArrayMemory<TIndex>(_MultiDrawIndirectMesh.RentIndexMemory,
                    (uint)sizeof(TIndex), pendingData.Indexes.Segment.Cast<QuadIndexes<TIndex>, TIndex>()),
                new BufferArrayMemory<TVertex>(_MultiDrawIndirectMesh.RentVertexMemory,
                    0u, pendingData.Vertexes.Segment.Cast<QuadVertexes<TVertex>, TVertex>()));

            return true;
        }

        private static void ConfigureMaterial(EntityManager entityManager, IEntity entity)
        {
            ProgramPipeline programPipeline = ProgramRegistry.Instance.Load("Resources/Shaders/PackedVertex.glsl", "Resources/Shaders/DefaultFragment.glsl");

            if (entity.TryFind(out Material? material))
            {
                if (material.Pipeline.Handle != programPipeline.Handle) material.Pipeline = programPipeline;
            }
            else entityManager.RegisterComponent(entity, material = new Material(programPipeline));

            //material.Textures.Add(TextureAtlas.Instance.Blocks ?? throw new NullReferenceException("Blocks texture array not initialized."));
        }

        #endregion
    }
}
