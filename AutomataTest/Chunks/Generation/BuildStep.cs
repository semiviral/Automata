using System;
using Automata.Numerics;

namespace AutomataTest.Chunks.Generation
{
    public abstract class BuildStep
    {
        public class Parameters
        {
            public int Seed { get; }
            public float Frequency { get; }
            public float Persistence { get; }
            public Vector3i Origin { get; }
            public Random SeededRandom { get; }

            public Parameters(int seed, float frequency, float persistence, Vector3i origin)
            {
                Seed = seed;
                Frequency = frequency;
                Persistence = persistence;
                Origin = origin;
                SeededRandom = new Random(seed);
            }
        }

        public abstract void Generate(Parameters parameters, Span<ushort> blocks);
    }
}
