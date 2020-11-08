#region

using System;
using System.Runtime.CompilerServices;
using Automata.Engine;
using Automata.Engine.Noise;
using Automata.Engine.Numerics;
using Automata.Game.Blocks;

#endregion


namespace Automata.Game.Chunks.Generation
{
    public class TerrainGenerationStep : IGenerationStep
    {
        public void Generate(Vector3i origin, IGenerationStep.Parameters parameters, Span<ushort> blocks)
        {
            Span<int> heightmap = stackalloc int[GenerationConstants.CHUNK_SIZE_SQUARED];

            int index = 0;

            for (int y = 0; y < GenerationConstants.CHUNK_SIZE; y++)
            {
                int heightmapIndex = 0;

                for (int z = 0; z < GenerationConstants.CHUNK_SIZE; z++)
                for (int x = 0; x < GenerationConstants.CHUNK_SIZE; x++, heightmapIndex++, index++)
                {
                    if (y == 0) heightmap[heightmapIndex] = CalculateHeight(origin.X + x, origin.Z + z, parameters.Frequency, parameters.Persistence);

                    int noiseHeight = heightmap[heightmapIndex];
                    int globalHeight = origin.Y + y;

                    if ((globalHeight < 4) && (globalHeight <= parameters.SeededRandom.Next(0, 4)))
                        blocks[index] = BlockRegistry.Instance.GetBlockID("Core:Bedrock");
                    else if ((noiseHeight < origin.Y)
                             || (CalculateCaveNoise(origin.X + x, origin.Y + y, origin.Z + z, parameters.Seed ^ 2, parameters.Seed ^ 3, parameters.Persistence)
                                 < 0.000225f)) blocks[index] = BlockRegistry.AirID;
                    else if (globalHeight == noiseHeight) blocks[index] = BlockRegistry.Instance.GetBlockID("Core:Grass");
                    else if ((globalHeight < noiseHeight) && (globalHeight >= (noiseHeight - 3))) // lay dirt up to 3 blocks below noise height
                    {
                        blocks[index] = parameters.SeededRandom.Next(0, 8) == 0
                            ? BlockRegistry.Instance.GetBlockID("Core:Dirt_Coarse")
                            : BlockRegistry.Instance.GetBlockID("Core:Dirt");
                    }
                    else if (globalHeight < (noiseHeight - 3))
                    {
                        blocks[index] = parameters.SeededRandom.Next(0, 100) == 0
                            ? BlockRegistry.Instance.GetBlockID("Core:Ore_Coal")
                            : BlockRegistry.Instance.GetBlockID("Core:Stone");
                    }
                    else blocks[index] = BlockRegistry.AirID;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float CalculateCaveNoise(int x, int y, int z, int noiseSeedA, int noiseSeedB, float persistence)
        {
            float currentHeight = (y + (((GenerationConstants.WORLD_HEIGHT / 4f) - (y * 1.25f)) * persistence)) * 0.85f;
            float heightDampener = AutomataMath.Unlerp(0f, GenerationConstants.WORLD_HEIGHT, currentHeight);
            float noiseA = OpenSimplexSlim.GetSimplex(noiseSeedA, 0.01f, x, y, z) * heightDampener;
            float noiseB = OpenSimplexSlim.GetSimplex(noiseSeedB, 0.01f, x, y, z) * heightDampener;
            float noiseAPow2 = MathF.Pow(noiseA, 2f);
            float noiseBPow2 = MathF.Pow(noiseB, 2f);

            return (noiseAPow2 + noiseBPow2) / 2f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int CalculateHeight(int x, int z, float frequency, float persistence)
        {
            float noise = OpenSimplexSlim.GetSimplex(GenerationConstants.Seed, frequency, x, z);
            float noiseHeight = AutomataMath.Unlerp(-1f, 1f, noise) * GenerationConstants.WORLD_HEIGHT;
            float modifiedNoiseHeight = noiseHeight + (((GenerationConstants.WORLD_HEIGHT / 2f) - (noiseHeight * 1.25f)) * persistence);

            return (int)modifiedNoiseHeight;
        }
    }
}
