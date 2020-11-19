using System;
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
using Silk.NET.Input.Common;

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

            InputManager.Instance.RegisterInputAction(() => _MultiDrawIndirectMesh?.ValidateAllocatorBlocks(), Key.F9);
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
                Stopwatch stopwatch = Stopwatch.StartNew();

                ProcessDrawElementsIndirectAllocationsImpl(entityManager);

                Log.Information($"{stopwatch.Elapsed.TotalMilliseconds:0.00}ms");
            }

            return ValueTask.CompletedTask;
        }


        #region State

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

        #endregion


        #region Data Processing

        [SkipLocalsInit]
        private unsafe void ProcessDrawElementsIndirectAllocationsImpl(EntityManager entityManager)
        {
            int drawIndirectAllocationsCount = (int)entityManager.GetComponentCount<DrawElementsIndirectAllocation<TIndex, TVertex>>();
            Span<DrawElementsIndirectCommand> commands = stackalloc DrawElementsIndirectCommand[drawIndirectAllocationsCount];
            Span<Matrix4x4> models = stackalloc Matrix4x4[drawIndirectAllocationsCount];

            int index = 0;

            foreach ((IEntity entity, DrawElementsIndirectAllocation<TIndex, TVertex> allocation) in
                entityManager.GetEntitiesWithComponents<DrawElementsIndirectAllocation<TIndex, TVertex>>())
            {
                if (allocation.Allocation is null)
                {
                    ThrowHelper.ThrowNullReferenceException("Allocation should not be null at this point.");
                }

                commands[index] = new DrawElementsIndirectCommand(allocation.Allocation!.VertexArrayMemory.Count, 1u,
                    (uint)(allocation.Allocation!.IndexesArrayMemory.Index / (nuint)sizeof(TIndex)),
                    (uint)(allocation.Allocation!.VertexArrayMemory.Index / (nuint)sizeof(TVertex)), (uint)index);

                models[index] = entity.Find<RenderModel>()?.Model ?? Matrix4x4.Identity;

                index += 1;
            }

            _MultiDrawIndirectMesh!.AllocateDrawCommands(commands);
            _MultiDrawIndirectMesh!.AllocateModelsData(models);

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
            if (!entity.TryFind(out DrawElementsIndirectAllocation<TIndex, TVertex>? drawIndirectAllocation))
            {
                drawIndirectAllocation = entityManager.RegisterComponent<DrawElementsIndirectAllocation<TIndex, TVertex>>(entity);
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
                $"Allocated new {nameof(DrawElementsIndirectAllocation<TIndex, TVertex>)}: {indexArrayMemory.MemoryOwner.Memory.Length} indexes, {vertexArrayMemory.MemoryOwner.Memory.Length} vertexes"));

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
