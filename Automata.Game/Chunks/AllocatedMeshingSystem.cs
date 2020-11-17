using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Automata.Engine;
using Automata.Engine.Diagnostics;
using Automata.Engine.Entities;
using Automata.Engine.Extensions;
using Automata.Engine.Rendering.Meshes;
using Automata.Engine.Rendering.OpenGL;
using Automata.Engine.Rendering.OpenGL.Buffers;
using Automata.Engine.Rendering.OpenGL.Shaders;
using Automata.Engine.Systems;
using Automata.Game.Chunks.Generation;
using Automata.Game.Chunks.Generation.Meshing;
using DiagnosticsProviderNS;

namespace Automata.Game.Chunks
{
    public class AllocatedMeshingSystem : ComponentSystem
    {
        private MultiDrawIndirectMesh? _MultiDrawIndirectMesh;

        public override void Registered(EntityManager entityManager)
        {
            _MultiDrawIndirectMesh = new MultiDrawIndirectMesh(GLAPI.Instance.GL, 750_000_000, 500_000_000);

            _MultiDrawIndirectMesh.AllocateVertexAttributes(true,
                new VertexAttribute<int>(0u, 1u, 0u),
                new VertexAttribute<int>(1u, 1u, 4u),
                new VertexAttribute<float>(2u + 0u, 4u, 0u, 0u, 1u));

            _MultiDrawIndirectMesh.FinalizeVertexArrayObject(0);

            Material material = new Material(ProgramRegistry.Instance.Load("Resources/Shaders/PackedVertex.glsl", "Resources/Shaders/DefaultFragment.glsl"));
            material.Textures.Add(TextureAtlas.Instance.Blocks!);

            entityManager.CreateEntity(
                material,
                new RenderMesh
                {
                    Mesh = _MultiDrawIndirectMesh
                });
        }

        public override ValueTask Update(EntityManager entityManager, TimeSpan delta)
        {
            bool recreateCommandBuffer = false;

            foreach ((IEntity entity, AllocatedMeshData mesh) in entityManager.GetEntitiesWithComponents<AllocatedMeshData>())
            {
                ProcessMeshData(entityManager, entity, mesh.Data);
                entityManager.RemoveComponent<AllocatedMeshData>(entity);
                recreateCommandBuffer = true;
            }

            if (recreateCommandBuffer) GenerateDrawElementsIndirectCommands(entityManager.GetComponents<DrawIndirectAllocation>());

            return ValueTask.CompletedTask;
        }

        private unsafe void GenerateDrawElementsIndirectCommands(IEnumerable<DrawIndirectAllocation> drawIndirectAllocations)
        {
            if (_MultiDrawIndirectMesh is null) ThrowHelper.ThrowNullReferenceException(nameof(_MultiDrawIndirectMesh));

            DrawIndirectAllocation[] allocations = drawIndirectAllocations.ToArray();
            Span<DrawElementsIndirectCommand> commands = stackalloc DrawElementsIndirectCommand[allocations.Length];

            for (int index = 0; index < allocations.Length; index++)
            {
                DrawIndirectAllocation drawIndirectAllocation = allocations[index];
                if (drawIndirectAllocation.Allocation is null) ThrowHelper.ThrowNullReferenceException(nameof(drawIndirectAllocation.Allocation));

                uint indexesStart = (uint)(drawIndirectAllocation.Allocation!.IndexesIndex / sizeof(uint));
                uint vertexesStart = (uint)drawIndirectAllocation.Allocation!.VertexesIndex;
                commands[index] = new DrawElementsIndirectCommand(drawIndirectAllocation.Allocation!.VertexCount, 1u, indexesStart, vertexesStart, (uint)index);
            }

            _MultiDrawIndirectMesh!.AllocateDrawElementsIndirectCommands(commands);
        }


        #region Mesh Preparation

        private void ProcessMeshData(EntityManager entityManager, IEntity entity, NonAllocatingQuadsMeshData<PackedVertex> pendingData)
        {
            Stopwatch stopwatch = DiagnosticsPool.Stopwatches.Rent();
            stopwatch.Restart();

            if (ApplyMeshMultiDraw(entityManager, entity, pendingData))
            {
                ConfigureMaterial(entityManager, entity);
                DiagnosticsProvider.CommitData<ChunkGenerationDiagnosticGroup, TimeSpan>(new ApplyMeshTime(stopwatch.Elapsed));
            }

            DiagnosticsPool.Stopwatches.Return(stopwatch);
        }

        private unsafe bool ApplyMeshMultiDraw(EntityManager entityManager, IEntity entity, NonAllocatingQuadsMeshData<PackedVertex> pendingData)
        {
            if (_MultiDrawIndirectMesh is null) throw new NullReferenceException("Mesh is null!");
            else if (pendingData.IsEmpty) return false;

            if (!entity.TryFind(out DrawIndirectAllocation? drawIndirectAllocation))
                drawIndirectAllocation = entityManager.RegisterComponent<DrawIndirectAllocation>(entity);

            drawIndirectAllocation.Allocation?.Dispose();

            int vertexArrayHeaderSize = sizeof(Vector4);

            int indexesSize = pendingData.Indexes.Count * sizeof(QuadIndexes);
            int vertexesSize = (pendingData.Vertexes.Count * sizeof(QuadVertexes<PackedVertex>)) + vertexArrayHeaderSize;
            IMemoryOwner<uint> indexesOwner = _MultiDrawIndirectMesh.RentIndexMemory(indexesSize, (nuint)sizeof(uint), out nuint indexesIndex);
            IMemoryOwner<byte> vertexesOwner = _MultiDrawIndirectMesh.RentVertexMemory<byte>(vertexesSize, 0u, out nuint vertexesIndex);

            drawIndirectAllocation.Allocation = new DrawIndirectAllocation.AllocationWrapper(indexesOwner, vertexesOwner, indexesIndex, vertexesIndex,
                (uint)(pendingData.Vertexes.Count * 4));

            _MultiDrawIndirectMesh.WaitForBufferFreeSync();
            MemoryMarshal.Cast<QuadIndexes, uint>(pendingData.Indexes.Segment).CopyTo(indexesOwner.Memory.Span);
            Span<byte> vertexes = vertexesOwner.Memory.Span;
            Vector4.One.Unroll<Vector4, byte>().CopyTo(vertexes);
            MemoryMarshal.AsBytes(pendingData.Vertexes.Segment).CopyTo(vertexes.Slice(vertexArrayHeaderSize));

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

            material.Textures.Add(TextureAtlas.Instance.Blocks ?? throw new NullReferenceException("Blocks texture array not initialized."));
        }

        #endregion
    }
}
