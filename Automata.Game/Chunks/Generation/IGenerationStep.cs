using System;
using Automata.Engine.Numerics;


namespace Automata.Game.Chunks.Generation
{
    public interface IGenerationStep
    {
        public record Parameters
        {
            public int Seed { get; }
            public Random SeededRandom { get; init; }

            public float Frequency { get; init; } = 0.0075f;
            public float Persistence { get; init; } = 0.65f;
            public float CaveThreshold { get; init; } = 0.000225f;

            public Parameters(int seed) => (Seed, SeededRandom) = (seed, new Random(seed));
        }

        public void Generate(Vector3i origin, Parameters parameters, Span<ushort> blocks);
    }
}
