using System;
using Automata.Engine.Components;

namespace Automata.Engine.Rendering.Meshes
{
    public class AllocatedMeshData<TIndex, TVertex> : Component, IDisposable
        where TIndex : unmanaged, IEquatable<TIndex>
        where TVertex : unmanaged, IEquatable<TVertex>
    {
        public NonAllocatingQuadsMeshData<TIndex, TVertex> Data { get; }

        public AllocatedMeshData(NonAllocatingQuadsMeshData<TIndex, TVertex> data) => Data = data;


        #region IDisposable

        public void Dispose()
        {
            Data.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
