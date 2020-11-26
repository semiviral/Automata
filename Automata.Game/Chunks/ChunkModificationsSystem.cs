using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Automata.Engine;
using Automata.Engine.Input;
using Automata.Game.Blocks;
using DiagnosticsProviderNS;
using Serilog;
using Silk.NET.Input;

namespace Automata.Game.Chunks
{
    public class ChunkModificationsSystem : ComponentSystem
    {
        public override void Registered(EntityManager entityManager)
        {
            DiagnosticsProvider.EnableGroup<ChunkModificationsDiagnosticGroup>();

            InputManager.Instance.RegisterInputAction(() => Log.Debug(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(ChunkModificationsSystem),
                $"Average update time: {DiagnosticsProvider.GetGroup<ChunkModificationsDiagnosticGroup>().Average():0.00}ms")), Key.ShiftLeft, Key.C);
        }

        [HandledComponents(EnumerationStrategy.All, typeof(Chunk))]
        public override ValueTask UpdateAsync(EntityManager entityManager, TimeSpan delta)
        {
            foreach (Chunk chunk in entityManager.GetComponents<Chunk>())
            {
                if (chunk.State is GenerationState.AwaitingMesh or GenerationState.Finished
                    && Array.TrueForAll(chunk.Neighbors, neighbor => neighbor?.State is <= GenerationState.AwaitingMesh or GenerationState.Finished)
                    && TryProcessChunkModifications(chunk))
                {
                    chunk.RemeshNeighborhood(true);
                }
            }

            return ValueTask.CompletedTask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryProcessChunkModifications(Chunk chunk)
        {
            if (chunk.Blocks is null)
            {
                ThrowHelper.ThrowNullReferenceException("Chunk must have blocks.");
            }

            bool modified = false;

            while (chunk.Modifications.TryTake(out ChunkModification? modification) && (chunk.Blocks![modification!.BlockIndex].ID != modification.BlockID))
            {
                chunk.Blocks[modification.BlockIndex] = new Block(modification.BlockID);
                modified = true;
            }

            return modified;
        }
    }
}
