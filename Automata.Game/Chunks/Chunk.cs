using System;
using System.Collections.Generic;
using System.Linq;
using Automata.Engine;
using Automata.Engine.Collections;
using Automata.Game.Blocks;

namespace Automata.Game.Chunks
{
    public class Chunk : Component
    {
        private GenerationState _State;

        public GenerationState State
        {
            get => _State;
            set
            {
#if DEBUG
                ValidateStateChange(value);
#endif

                _State = value;
            }
        }

        // todo having this variable is dumb, but it's simple for now. replace later.
        internal Entity Entity { get; set; }

        public Palette<Block>? Blocks { get; set; }
        public Chunk?[] Neighbors { get; } = new Chunk?[6];
        public ConcurrentChannel<ChunkModification> Modifications { get; } = new ConcurrentChannel<ChunkModification>(true, false);
        public int TimesMeshed { get; set; }

        public bool IsGenerating => State is GenerationState.GeneratingTerrain or GenerationState.GeneratingStructures or GenerationState.GeneratingMesh;
        public IEnumerable<Palette<Block>?> NeighborBlocks() => Neighbors.Select(chunk => chunk?.Blocks);

        public void DangerousRemeshNeighborhood()
        {
            State = GenerationState.AwaitingMesh;

            foreach (Chunk? neighbor in Neighbors)
            {
                if (neighbor?.State is > GenerationState.AwaitingMesh)
                {
                    neighbor.State = GenerationState.AwaitingMesh;
                }
            }
        }


        #region Debug

        private void ValidateStateChange(GenerationState newState)
        {
            switch (newState)
            {
                case GenerationState.Inactive:
                case GenerationState.AwaitingTerrain when State is GenerationState.Inactive:
                case GenerationState.AwaitingStructures when State is GenerationState.GeneratingTerrain:
                case GenerationState.AwaitingMesh when State is GenerationState.GeneratingStructures or GenerationState.Finished:
                case GenerationState.Finished when State is GenerationState.GeneratingMesh:
                case GenerationState.GeneratingTerrain when State is GenerationState.AwaitingTerrain:
                case GenerationState.GeneratingStructures when State is GenerationState.AwaitingStructures:
                case GenerationState.GeneratingMesh when State is GenerationState.AwaitingMesh: break;
                default: throw new InvalidOperationException("Attempted an invalid state modification");
            }
        }

        #endregion


        #region IDisposable

        public void RegionDispose()
        {
            State = GenerationState.Inactive;
            Modifications.Dispose();
            Blocks?.Dispose();
            Array.Clear(Neighbors, 0, Neighbors.Length);
        }

        #endregion
    }
}
