#region

using System;
using Automata.Engine;
using Automata.Engine.Noise;
using Automata.Engine.Numerics;
using Automata.Game.Blocks;

#endregion


namespace Automata.Game.Chunks.Generation
{
    public class TerrainGenerationStep : GenerationStep
    {
        public override void Generate(Vector3i origin, Parameters parameters, Span<ushort> blocks)
        {
            Span<int> heightmap = stackalloc int[GenerationConstants.CHUNK_SIZE_SQUARED];
            Span<float> cavemap = stackalloc float[GenerationConstants.CHUNK_SIZE_CUBED];

            int index = 0;

            for (int x = 0; x < GenerationConstants.CHUNK_SIZE; x++)
            for (int z = 0; z < GenerationConstants.CHUNK_SIZE; z++)
            {
                int heightmapIndex = Vector2i.Project1D(x, z, GenerationConstants.CHUNK_SIZE);

                heightmap[heightmapIndex] = CalculateHeight(new Vector2i(origin.X + x, origin.Z + z), parameters.Frequency,
                    parameters.Persistence);

                for (int y = 0; y < GenerationConstants.CHUNK_SIZE; y++, index++)
                {
                    Vector3i globalPosition = origin + new Vector3i(x, y, z);
                    cavemap[index] = CalculateCaveNoise(globalPosition, parameters.Seed ^ 2, parameters.Seed ^ 3, parameters.Persistence);
                }
            }

            for (index = 0; index < GenerationConstants.CHUNK_SIZE_CUBED; index++)
            {
                Vector3i localPosition = Vector3i.Project3D(index, GenerationConstants.CHUNK_SIZE);

                int heightmapIndex = Vector2i.Project1D(new Vector2i(localPosition.X, localPosition.Z), GenerationConstants.CHUNK_SIZE);
                int noiseHeight = heightmap[heightmapIndex];

                BlockRegistry blockRegistry = BlockRegistry.Instance;
                int globalPositionY = origin.Y + localPosition.Y;

                if ((globalPositionY < 4) && (globalPositionY <= parameters.SeededRandom.Next(0, 4)))
                {
                    blocks[index] = blockRegistry.GetBlockID("Core:Bedrock");
                }
                else if ((noiseHeight < origin.Y) || (cavemap[index] < 0.000225f))
                {
                    blocks[index] = BlockRegistry.AirID;
                }
                else if (globalPositionY == noiseHeight)
                {
                    blocks[index] = blockRegistry.GetBlockID("Core:Grass");
                }
                else if ((globalPositionY < noiseHeight) && (globalPositionY >= (noiseHeight - 3))) // lay dirt up to 3 blocks below noise height
                {
                    blocks[index] = parameters.SeededRandom.Next(0, 8) == 0
                        ? blockRegistry.GetBlockID("Core:Dirt_Coarse")
                        : blockRegistry.GetBlockID("Core:Dirt");
                }
                else if (globalPositionY < (noiseHeight - 3))
                {
                    blocks[index] = parameters.SeededRandom.Next(0, 100) == 0
                        ? blockRegistry.GetBlockID("Core:Ore_Coal")
                        : blockRegistry.GetBlockID("Core:Stone");
                }
                else
                {
                    blocks[index] = BlockRegistry.AirID;
                }
            }
        }

        private static float CalculateCaveNoise(Vector3i globalPosition, int noiseSeedA, int noiseSeedB, float persistence)
        {
            float currentHeight = (globalPosition.Y + (((GenerationConstants.WORLD_HEIGHT / 4f) - (globalPosition.Y * 1.25f)) * persistence)) * 0.85f;
            float heightDampener = AutomataMath.Unlerp(0f, GenerationConstants.WORLD_HEIGHT, currentHeight);
            float noiseA = OpenSimplexSlim.GetSimplex(noiseSeedA, 0.01f, globalPosition) * heightDampener;
            float noiseB = OpenSimplexSlim.GetSimplex(noiseSeedB, 0.01f, globalPosition) * heightDampener;
            float noiseAPow2 = (float)Math.Pow(noiseA, 2f);
            float noiseBPow2 = (float)Math.Pow(noiseB, 2f);

            return (noiseAPow2 + noiseBPow2) / 2f;
        }

        private static int CalculateHeight(Vector2i globalPosition, float frequency, float persistence)
        {
            float noise = OpenSimplexSlim.GetSimplex(GenerationConstants.Seed, frequency, globalPosition);
            float noiseHeight = AutomataMath.Unlerp(-1f, 1f, noise) * GenerationConstants.WORLD_HEIGHT;
            float modifiedNoiseHeight = noiseHeight + (((GenerationConstants.WORLD_HEIGHT / 2f) - (noiseHeight * 1.25f)) * persistence);

            return (int)modifiedNoiseHeight;
        }
    }
}
