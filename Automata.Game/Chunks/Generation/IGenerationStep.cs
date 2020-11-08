#region

using System;
using Automata.Engine.Numerics;

#endregion


namespace Automata.Game.Chunks.Generation
{
    public interface IGenerationStep
    {
        public class Parameters
        {
            public int Seed { get; }
            public float Frequency { get; }
            public float Persistence { get; }
            public Random SeededRandom { get; }

            public Parameters(int seed, float frequency, float persistence)
            {
                Seed = seed;
                Frequency = frequency;
                Persistence = persistence;
                SeededRandom = new Random(seed);
            }
        }

        public void Generate(Vector3i origin, Parameters parameters, Span<ushort> blocks);
    }
}
