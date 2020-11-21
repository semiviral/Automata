using System;

namespace Automata.Engine.Rendering.Meshes
{
    public class AllocatedMeshData<TIndex, TVertex> : Component
        where TIndex : unmanaged, IEquatable<TIndex>
        where TVertex : unmanaged, IEquatable<TVertex>
    {
        public NonAllocatingQuadsMeshData<TIndex, TVertex> Data { get; }

        public AllocatedMeshData(NonAllocatingQuadsMeshData<TIndex, TVertex> data) => Data = data;


        #region IDisposable

        protected override void CleanupManagedResources() => Data.Dispose();

        #endregion
    }
}
