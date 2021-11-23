using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Automata.Engine.Collections;
using Automata.Engine.Input;
using Automata.Engine.Rendering.OpenGL;
using Automata.Engine.Rendering.OpenGL.Buffers;
using Automata.Engine.Rendering.OpenGL.Shaders;
using Automata.Engine.Rendering.OpenGL.Textures;
using Serilog;
using Silk.NET.Input;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.Meshes
{
    public class AllocatedMeshingSystem<TIndex, TVertex> : ComponentSystem
        where TIndex : unmanaged, IEquatable<TIndex>
        where TVertex : unmanaged, IEquatable<TVertex>
    {
        private readonly MultiDrawIndirectMesh _MultiDrawIndirectMesh;
        private readonly Material _MultiDrawIndirectMeshMaterial;

        public AllocatedMeshingSystem(World world) : base(world)
        {
            DrawElementsType draw_elements_type;

            if (typeof(TIndex) == typeof(byte))
            {
                draw_elements_type = DrawElementsType.UnsignedByte;
            }
            else if (typeof(TIndex) == typeof(ushort))
            {
                draw_elements_type = DrawElementsType.UnsignedShort;
            }
            else if (typeof(TIndex) == typeof(uint))
            {
                draw_elements_type = DrawElementsType.UnsignedInt;
            }
            else
            {
                throw new NotSupportedException("Does not support specified index type.");
            }

            // todo this system shouldn't contain its own mesh.
            //  remark: break this out into its own component, possibly with an ID that DrawElementsIndirectAllocation
            //  can use to refer to which mesh it belongs to.
            //
            //  the expectaion being that you compose the mesh component, and keep the ID around to use with your own
            //  systems and such.
            _MultiDrawIndirectMesh = new MultiDrawIndirectMesh(GLAPI.Instance.GL, 500_000_000, draw_elements_type);

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

        public override async ValueTask UpdateAsync(EntityManager entityManager, TimeSpan delta)
        {
            if (TryGetAggregateAllocationTasks(entityManager, out NonAllocatingList<Task>? tasks))
            {
                await Task.WhenAll(tasks);
                ProcessDrawElementsIndirectAllocations(entityManager);
            }

            tasks?.Dispose();
        }

        private bool TryGetAggregateAllocationTasks(EntityManager entityManager, [NotNullWhen(true)] out NonAllocatingList<Task>? tasks)
        {
            nint allocated_mesh_data_count = entityManager.GetComponentCount<AllocatedMeshData<TIndex, TVertex>>();

            if (allocated_mesh_data_count is 0)
            {
                tasks = null;
                return false;
            }

            tasks = new NonAllocatingList<Task>((int)allocated_mesh_data_count);
            bool recreate_command_buffer = false;

            foreach ((Entity entity, AllocatedMeshData<TIndex, TVertex> mesh) in
                entityManager.GetEntitiesWithComponents<AllocatedMeshData<TIndex, TVertex>>())
            {
                unsafe Task create_draw_indirect_allocation_allocation_impl_impl(DrawElementsIndirectAllocation<TIndex, TVertex> pendingAllocation)
                {
                    pendingAllocation.Allocation?.Dispose();

                    BufferMemory<TIndex> index_array_memory = _MultiDrawIndirectMesh.RentBufferMemory<TIndex>((nuint)sizeof(TIndex),
                        MemoryMarshal.Cast<QuadIndexes<TIndex>, TIndex>(mesh.Data.Indexes.Segment));

                    BufferMemory<TVertex> vertex_array_memory = _MultiDrawIndirectMesh.RentBufferMemory<TVertex>((nuint)sizeof(TVertex),
                        MemoryMarshal.Cast<QuadVertexes<TVertex>, TVertex>(mesh.Data.Vertexes.Segment));

                    pendingAllocation.Allocation = new MeshMemory<TIndex, TVertex>(index_array_memory, vertex_array_memory);

                    return Task.CompletedTask;
                }

                if (!mesh.Data.IsEmpty)
                {
                    if (!entity.TryComponent(out DrawElementsIndirectAllocation<TIndex, TVertex>? allocation))
                        allocation = entityManager.RegisterComponent<DrawElementsIndirectAllocation<TIndex, TVertex>>(entity);

                    tasks.Add(create_draw_indirect_allocation_allocation_impl_impl(allocation));
                    ConfigureMaterial(entityManager, entity);
                    recreate_command_buffer = true;
                }

                entityManager.RemoveComponent<AllocatedMeshData<TIndex, TVertex>>(entity);
            }

            return recreate_command_buffer;
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
            int draw_indirect_allocations_count = (int)entityManager.GetComponentCount<DrawElementsIndirectAllocation<TIndex, TVertex>>();
            DrawElementsIndirectCommand[] commands = ArrayPool<DrawElementsIndirectCommand>.Shared.Rent(draw_indirect_allocations_count);
            Matrix4x4[] models = ArrayPool<Matrix4x4>.Shared.Rent(draw_indirect_allocations_count);

            int index = 0;

            foreach ((Entity entity, DrawElementsIndirectAllocation<TIndex, TVertex> allocation) in
                entityManager.GetEntitiesWithComponents<DrawElementsIndirectAllocation<TIndex, TVertex>>())
            {
                DrawElementsIndirectCommand draw_elements_indirect_command = new DrawElementsIndirectCommand(allocation.Allocation!.IndexesMemory.Count, 1u,
                    (uint)(allocation.Allocation!.IndexesMemory.Index / (nuint)sizeof(TIndex)),
                    (uint)(allocation.Allocation!.VertexMemory.Index / (nuint)sizeof(TVertex)), (uint)index);

                commands[index] = draw_elements_indirect_command;
                models[index] = entity.Component<Transform>()?.Matrix ?? Matrix4x4.Identity;
                index += 1;
            }

            // make sure we slice the rentals here, since they're subject to arbitrary sizing rules (and may not be the exact requested minimum size).
            _MultiDrawIndirectMesh.AllocateDrawCommands(new Span<DrawElementsIndirectCommand>(commands, 0, draw_indirect_allocations_count));
            _MultiDrawIndirectMesh.AllocateModelsData(new Span<Matrix4x4>(models, 0, draw_indirect_allocations_count));
            ArrayPool<DrawElementsIndirectCommand>.Shared.Return(commands);
            ArrayPool<Matrix4x4>.Shared.Return(models);

            Log.Verbose(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(AllocatedMeshingSystem<TIndex, TVertex>),
                $"Allocated {draw_indirect_allocations_count} {nameof(DrawElementsIndirectCommand)}"));
        }

        private static void ConfigureMaterial(EntityManager entityManager, Entity entity)
        {
            ProgramPipeline program_pipeline = ProgramRegistry.Instance.Load("Resources/Shaders/PackedVertex.glsl", "Resources/Shaders/DefaultFragment.glsl");

            if (entity.TryComponent(out Material? material))
            {
                if (material.Pipeline.Handle != program_pipeline.Handle)
                {
                    material.Pipeline = program_pipeline;
                }
            }
            else
            {
                entityManager.RegisterComponent(entity, new Material(program_pipeline));
            }
        }

        #endregion Data Processing
    }
}
