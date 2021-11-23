using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Automata.Engine.Extensions;
using Automata.Engine.Noise;
using Automata.Engine.Numerics;
using Automata.Game.Blocks;

namespace Automata.Game.Chunks.Generation
{
    public class TerrainGenerationStep : IGenerationStep
    {
        [SkipLocalsInit]
        public void Generate(Vector3<int> origin, IGenerationStep.Parameters parameters, Span<ushort> blocks)
        {
            Span<int> heightmap = stackalloc int[GenerationConstants.CHUNK_SIZE_SQUARED];

            int index = 0;

            for (int y = 0; y < GenerationConstants.CHUNK_SIZE; y++)
            {
                int heightmap_index = 0;

                for (int z = 0; z < GenerationConstants.CHUNK_SIZE; z++)
                for (int x = 0; x < GenerationConstants.CHUNK_SIZE; x++, heightmap_index++, index++)
                {
                    Vector3<int> global = origin + new Vector3<int>(x, y, z);

                    if (y == 0)
                    {
                        heightmap[heightmap_index] = CalculateHeight(new Vector2(global.X, global.Z), parameters.Frequency, parameters.Persistence);
                    }

                    int noise_height = heightmap[heightmap_index];

                    if ((global.Y < 4) && (global.Y <= parameters.SeededRandom.Next(0, 4)))
                    {
                        blocks[index] = BlockRegistry.Instance.GetBlockID("Core:Bedrock");
                    }
                    else if ((noise_height < origin.Y) || (CalculateCaveNoise(global.Convert<float>(), parameters) < parameters.CaveThreshold))
                    {
                        blocks[index] = BlockRegistry.AirID;
                    }
                    else if (global.Y == noise_height)
                    {
                        blocks[index] = BlockRegistry.Instance.GetBlockID("Core:Grass");
                    }
                    else if ((global.Y < noise_height) && (global.Y >= (noise_height - 3))) // lay dirt up to 3 blocks below noise height
                    {
                        blocks[index] = parameters.SeededRandom.Next(0, 8) == 0
                            ? BlockRegistry.Instance.GetBlockID("Core:Dirt_Coarse")
                            : BlockRegistry.Instance.GetBlockID("Core:Dirt");
                    }
                    else if (global.Y < (noise_height - 3))
                    {
                        blocks[index] = BlockRegistry.Instance.GetBlockID("Core:Stone");
                    }
                    else
                    {
                        blocks[index] = BlockRegistry.AirID;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float CalculateCaveNoise(Vector3<float> global, IGenerationStep.Parameters parameters)
        {
            float current_height = (global.Y + (((GenerationConstants.WORLD_HEIGHT / 4f) - (global.Y * 1.25f)) * parameters.Persistence)) * 0.85f;
            float height_dampener = current_height.Unlerp(0f, GenerationConstants.WORLD_HEIGHT);

            float sample_a = OpenSimplexSlim.GetSimplex(parameters.Seed ^ 2, parameters.Frequency, global.AsIntrinsic()) * height_dampener;
            float sample_b = OpenSimplexSlim.GetSimplex(parameters.Seed ^ 3, parameters.Frequency, global.AsIntrinsic()) * height_dampener;
            sample_a *= sample_a;
            sample_b *= sample_b;

            return (sample_a + sample_b) * 0.5f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int CalculateHeight(Vector2 xz, float frequency, float persistence)
        {
            float noise = OpenSimplexSlim.GetSimplex(GenerationConstants.Seed, frequency, xz);
            float noise_height = noise.Unlerp(-1f, 1f) * GenerationConstants.WORLD_HEIGHT;
            float modified_noise_height = noise_height + (((GenerationConstants.WORLD_HEIGHT / 2f) - (noise_height * 1.25f)) * persistence);

            return (int)modified_noise_height;
        }
    }
}
