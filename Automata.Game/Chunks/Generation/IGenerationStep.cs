using System;
using Automata.Engine.Numerics;

namespace Automata.Game.Chunks.Generation
{
    public interface IGenerationStep
    {
        public sealed record Parameters
        {
            public int Seed { get; }
            public Random SeededRandom { get; init; }

            public float Frequency { get; init; } = 0.0075f;
            public float Persistence { get; init; } = 0.65f;
            public float CaveThreshold { get; init; } = 0.000225f;

            public Parameters(int seed, int randomSeed) => (Seed, SeededRandom) = (seed, new Random(randomSeed));
        }

        public void Generate(Vector3<int> origin, Parameters parameters, Span<ushort> blocks);
    }
}
