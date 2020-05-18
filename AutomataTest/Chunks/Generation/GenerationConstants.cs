#region

using Automata.Numerics;

#endregion

namespace AutomataTest.Chunks.Generation
{
    public static class GenerationConstants
    {
        public const float _FREQUENCY = 0.0075f;
        public const float PERSISTENCE = 0.6f;

        public const int CHUNK_SIZE_BIT_SHIFT = 6;
        public const int CHUNK_SIZE_BIT_MASK = (1 << CHUNK_SIZE_BIT_SHIFT) - 1;

        public const int CHUNK_SIZE = 1 << (CHUNK_SIZE_BIT_SHIFT - 1);
        public const int CHUNK_SIZE_SQUARED = CHUNK_SIZE * CHUNK_SIZE;
        public const int CHUNK_SIZE_CUBED = CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE;
        public const int WORLD_HEIGHT_IN_CHUNKS = 8;
        public const int WORLD_HEIGHT = CHUNK_SIZE * WORLD_HEIGHT_IN_CHUNKS;

        public static int Seed { get; set; }

        public static readonly int[] IndexStepByNormalIndex =
        {
            1,
            CHUNK_SIZE_SQUARED,
            CHUNK_SIZE,
            -1,
            -CHUNK_SIZE_SQUARED,
            -CHUNK_SIZE,
        };

        public static readonly Vector3i[] NormalVectorByIteration =
        {
            new Vector3i(1, 0, 0),
            new Vector3i(0, 1, 0),
            new Vector3i(0, 0, 1),
            new Vector3i(-1, 0, 0),
            new Vector3i(0, -1, 0),
            new Vector3i(0, 0, -1),
        };

        public static readonly int[] NormalByIteration =
        {
            0b01_01_11_000000_000000_000000,
            0b01_11_01_000000_000000_000000,
            0b11_01_01_000000_000000_000000,
            0b01_01_00_000000_000000_000000,
            0b01_00_01_000000_000000_000000,
            0b00_01_01_000000_000000_000000,
        };

        public static readonly int[][] VertexesByIteration =
        {
            // z y x
            new[]
            {
                // East       z      y      x
                0b01_01_11_000001_000000_000001,
                0b01_01_11_000000_000000_000001,
                0b01_01_11_000001_000001_000001,
                0b01_01_11_000000_000001_000001,
            },
            new[]
            {
                // Up        z      y      x
                0b01_11_01_000001_000001_000000,
                0b01_11_01_000001_000001_000001,
                0b01_11_01_000000_000001_000000,
                0b01_11_01_000000_000001_000001,
            },
            new[]
            {
                // North    z      y      x
                0b11_01_01_000000_000000_000001,
                0b11_01_01_000001_000000_000001,
                0b11_01_01_000000_000001_000001,
                0b11_01_01_000001_000001_000001,
            },
            new[]
            {
                // West
                0b01_01_00_000000_000000_000000,
                0b01_01_00_000001_000000_000000,
                0b01_01_00_000000_000001_000000,
                0b01_01_00_000001_000001_000000,
            },
            new[]
            {
                // Down
                0b01_00_01_000001_000000_000001,
                0b01_00_01_000001_000000_000000,
                0b01_00_01_000000_000000_000001,
                0b01_00_01_000000_000000_000000,
            },
            new[]
            {
                // South
                0b00_01_01_000000_000000_000001,
                0b00_01_01_000000_000000_000000,
                0b00_01_01_000000_000001_000001,
                0b00_01_01_000000_000001_000000,
            },
        };
    }
}
