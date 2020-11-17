using System;
using Automata.Engine.Components;
using Automata.Engine.Rendering.Meshes;
using Automata.Game.Chunks.Generation.Meshing;

namespace Automata.Game.Chunks
{
    public class AllocatedMeshData : Component, IDisposable
    {
        public NonAllocatingQuadsMeshData<PackedVertex> Data { get; }

        public AllocatedMeshData(NonAllocatingQuadsMeshData<PackedVertex> data) => Data = data;


        #region IDisposable

        public void Dispose()
        {
            Data.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
