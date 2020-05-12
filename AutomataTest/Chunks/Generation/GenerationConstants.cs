#region

using System.Numerics;

#endregion

namespace AutomataTest.Chunks.Generation
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


        // '8' is the 'numthreads[]' value in the compute shader
        public const int CHUNK_THREAD_GROUP_SIZE = CHUNK_SIZE / 8;

        public static readonly int[] IndexStepByNormalIndex =
        {
            1,
            CHUNK_SIZE_SQUARED,
            CHUNK_SIZE,
            -1,
            -CHUNK_SIZE_SQUARED,
            -CHUNK_SIZE,
        };

        public static readonly Vector3[] NormalVectorByIteration =
        {
            new Vector3(1f, 0f, 0f),
            new Vector3(0f, 1f, 0f),
            new Vector3(0f, 0f, 1f),
            new Vector3(-1f, 0f, 0f),
            new Vector3(0f, -1f, 0f),
            new Vector3(0f, 0f, -1f),
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

        public static readonly int[][] VerticesByIteration =
        {
            new[]
            {
                // East
                0b000001_000000_000001,
                0b000001_000001_000001,
                0b000000_000000_000001,
                0b000000_000001_000001,
            },
            new[]
            {
                // Up
                0b000001_000001_000000,
                0b000000_000001_000000,
                0b000001_000001_000001,
                0b000000_000001_000001,
            },
            new[]
            {
                // North
                0b000001_000000_000000,
                0b000001_000001_000000,
                0b000001_000000_000001,
                0b000001_000001_000001,
            },
            new[]
            {
                // West
                0b000000_000000_000000,
                0b000000_000001_000000,
                0b000001_000000_000000,
                0b000001_000001_000000,
            },
            new[]
            {
                // Down
                0b000000_000000_000000,
                0b000001_000000_000000,
                0b000000_000000_000001,
                0b000001_000000_000001,
            },
            new[]
            {
                // South
                0b000000_000000_000001,
                0b000000_000001_000001,
                0b000000_000000_000000,
                0b000000_000001_000000,
            },
        };

        public static readonly int[][] UVIndexAdjustments =
        {
            new[]
            {
                -1, // iterating x axis
                1,
                0,
            },
            new[]
            {
                0,
                -1, // iterating y axis
                1,
            },
            new[]
            {
                0,
                1,
                -1, // iterating z axis
            }
        };
    }
}
