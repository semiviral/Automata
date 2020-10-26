using System.Numerics;
using Silk.NET.Vulkan;

namespace Automata.Game.Chunks.Generation
{
    public static class GenerationConstants
    {
        public const float FREQUENCY = 0.0075f;
        public const float PERSISTENCE = 0.6f;

        public const int CHUNK_SIZE_BIT_SHIFT = 6;
        public const int CHUNK_SIZE_BIT_MASK = (1 << CHUNK_SIZE_BIT_SHIFT) - 1;

        public const int CHUNK_SIZE = 1 << (CHUNK_SIZE_BIT_SHIFT - 1);
        public const int CHUNK_SIZE_SQUARED = CHUNK_SIZE * CHUNK_SIZE;
        public const int CHUNK_SIZE_CUBED = CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE;
        public const int CHUNK_RADIUS = CHUNK_SIZE / 2;
        public const int WORLD_HEIGHT_IN_CHUNKS = 8;
        public const int WORLD_HEIGHT = CHUNK_SIZE * WORLD_HEIGHT_IN_CHUNKS;

        public const int OVERLAP_SIZE = CHUNK_SIZE + 1;
        public const int OVERLAP_SIZE_SQUARED = OVERLAP_SIZE * OVERLAP_SIZE;
        public const int OVERLAP_SIZE_CUBED = OVERLAP_SIZE * OVERLAP_SIZE * OVERLAP_SIZE;

        public static int Seed { get; set; }
    }
}
