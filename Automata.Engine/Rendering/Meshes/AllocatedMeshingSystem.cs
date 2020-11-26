using System;
using System.Buffers;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Automata.Engine.Input;
using Automata.Engine.Rendering.OpenGL;
using Automata.Engine.Rendering.OpenGL.Buffers;
using Automata.Engine.Rendering.OpenGL.Shaders;
using Automata.Engine.Rendering.OpenGL.Textures;
using Serilog;
using Silk.NET.Input;

namespace Automata.Engine.Rendering.Meshes
{
    public class AllocatedMeshingSystem<TIndex, TVertex> : ComponentSystem
        where TIndex : unmanaged, IEquatable<TIndex>
        where TVertex : unmanaged, IEquatable<TVertex>
    {
        private readonly MultiDrawIndirectMesh<TIndex, TVertex> _MultiDrawIndirectMesh;
        private readonly Material _MultiDrawIndirectMeshMaterial;

        public AllocatedMeshingSystem()
        {
            _MultiDrawIndirectMesh = new MultiDrawIndirectMesh<TIndex, TVertex>(GLAPI.Instance.GL, 300_000_000, 200_000_000);

            _MultiDrawIndirectMeshMaterial =
                new Material(ProgramRegistry.Instance.Load("Resources/Shaders/PackedVertex.glsl", "Resources/Shaders/DefaultFragment.glsl"));
        }

        public override void Registered(EntityManager entityManager)
        {
            entityManager.CreateEntity(
                _MultiDrawIndirectMeshMaterial,
                new RenderMesh
                {
                    Mesh = _MultiDrawIndirectMesh
                });

            InputManager.Instance.RegisterInputAction(() => _MultiDrawIndirectMesh.ValidateAllocatorBlocks(), Key.F9);
        }

        public override ValueTask UpdateAsync(EntityManager entityManager, TimeSpan delta)
        {
            bool recreateCommandBuffer = false;

            foreach ((Entity entity, AllocatedMeshData<TIndex, TVertex> mesh) in entityManager.GetEntitiesWithComponents<AllocatedMeshData<TIndex, TVertex>>())
            {
                if (TryAllocateMesh(entityManager, entity, mesh.Data))
                {
                    ConfigureMaterial(entityManager, entity);
                    recreateCommandBuffer = true;
                }

                entityManager.RemoveComponent<AllocatedMeshData<TIndex, TVertex>>(entity);
            }

            if (recreateCommandBuffer)
            {
                _MultiDrawIndirectMesh.DrawSync?.BusyWaitCPU();
                ProcessDrawElementsIndirectAllocations(entityManager);
            }

            return ValueTask.CompletedTask;
        }


        #region State

        public void SetTexture(string key, Texture texture)
        {
            if (!_MultiDrawIndirectMeshMaterial.Textures.ContainsKey(key))
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
            _MultiDrawIndirectMesh.AllocateVertexAttributes(replace, attributes);

            if (finalize)
            {
                _MultiDrawIndirectMesh.FinalizeVertexArrayObject();
            }
        }

        public void FinalizeVertexArrayObject() => _MultiDrawIndirectMesh.FinalizeVertexArrayObject();

        #endregion State


        #region Data Processing

        [SkipLocalsInit]
        private unsafe void ProcessDrawElementsIndirectAllocations(EntityManager entityManager)
        {
            // rent from array pools to avoid allocations where possible
            //
            // we could use stackalloc here and work with stack memory directly, but that's liable to end up in a StackOverflowException for
            // very large render distances
            int drawIndirectAllocationsCount = (int)entityManager.GetComponentCount<DrawElementsIndirectAllocation<TIndex, TVertex>>();
            DrawElementsIndirectCommand[] commands = ArrayPool<DrawElementsIndirectCommand>.Shared.Rent(drawIndirectAllocationsCount);
            Matrix4x4[] models = ArrayPool<Matrix4x4>.Shared.Rent(drawIndirectAllocationsCount);

            int index = 0;

            foreach ((Entity entity, DrawElementsIndirectAllocation<TIndex, TVertex> allocation) in
                entityManager.GetEntitiesWithComponents<DrawElementsIndirectAllocation<TIndex, TVertex>>())
            {
                Debug.Assert(allocation.Allocation is not null);

                DrawElementsIndirectCommand drawElementsIndirectCommand = new DrawElementsIndirectCommand(allocation.Allocation.IndexesArrayMemory.Count, 1u,
                    (uint)(allocation.Allocation.IndexesArrayMemory.Index / (nuint)sizeof(TIndex)),
                    (uint)(allocation.Allocation.VertexArrayMemory.Index / (nuint)sizeof(TVertex)), (uint)index);

                commands[index] = drawElementsIndirectCommand;
                models[index] = entity.Find<Transform>()?.Matrix ?? Matrix4x4.Identity;
                index += 1;

                Log.Verbose(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(AllocatedMeshingSystem<TIndex, TVertex>), drawElementsIndirectCommand));
            }

            // make sure we slice the rentals here, since they're subject to arbitrary sizing rules (and may not be the exact requested minimum size).
            _MultiDrawIndirectMesh.AllocateDrawCommands(new Span<DrawElementsIndirectCommand>(commands, 0, drawIndirectAllocationsCount));
            _MultiDrawIndirectMesh.AllocateModelsData(new Span<Matrix4x4>(models, 0, drawIndirectAllocationsCount));
            ArrayPool<DrawElementsIndirectCommand>.Shared.Return(commands);
            ArrayPool<Matrix4x4>.Shared.Return(models);

            Log.Verbose(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(AllocatedMeshingSystem<TIndex, TVertex>),
                $"Allocated {drawIndirectAllocationsCount} {nameof(DrawElementsIndirectCommand)}"));
        }

        private unsafe bool TryAllocateMesh(EntityManager entityManager, Entity entity, NonAllocatingQuadsMeshData<TIndex, TVertex> pendingData)
        {
            if (pendingData.IsEmpty)
            {
                return false;
            }

            // if the entity doesn't have the required component, make sure we add it
            if (!entity.TryFind(out DrawElementsIndirectAllocation<TIndex, TVertex>? drawIndirectAllocation))
            {
                drawIndirectAllocation = entityManager.RegisterComponent<DrawElementsIndirectAllocation<TIndex, TVertex>>(entity);
            }

            // we make sure to dispose the old allocation to free the memory in the pool
            drawIndirectAllocation.Allocation?.Dispose();

            BufferArrayMemory<TIndex> indexArrayMemory = _MultiDrawIndirectMesh.RentIndexBufferArrayMemory((nuint)sizeof(TIndex),
                MemoryMarshal.Cast<QuadIndexes<TIndex>, TIndex>(pendingData.Indexes.Segment));

            BufferArrayMemory<TVertex> vertexArrayMemory = _MultiDrawIndirectMesh.RentVertexBufferArrayMemory((nuint)sizeof(TVertex),
                MemoryMarshal.Cast<QuadVertexes<TVertex>, TVertex>(pendingData.Vertexes.Segment));

            drawIndirectAllocation.Allocation = new MeshArrayMemory<TIndex, TVertex>(indexArrayMemory, vertexArrayMemory);

            Log.Verbose(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(AllocatedMeshingSystem<TIndex, TVertex>),
                $"Allocated new {nameof(DrawElementsIndirectAllocation<TIndex, TVertex>)}: {indexArrayMemory.MemoryOwner.Memory.Length} indexes, {vertexArrayMemory.MemoryOwner.Memory.Length} vertexes"));

            return true;
        }

        private static void ConfigureMaterial(EntityManager entityManager, Entity entity)
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

        #endregion Data Processing
    }
}
