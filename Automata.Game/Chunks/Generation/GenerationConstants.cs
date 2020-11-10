namespace Automata.Game.Chunks.Generation
{
    public static class GenerationConstants
    {
        public const int CHUNK_SIZE_SHIFT = 6;
        public const int CHUNK_SIZE_MASK = (1 << CHUNK_SIZE_SHIFT) - 1;

        public const int CHUNK_SIZE = 1 << (CHUNK_SIZE_SHIFT - 1);
        public const int CHUNK_SIZE_SQUARED = CHUNK_SIZE * CHUNK_SIZE;
        public const int CHUNK_SIZE_CUBED = CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE;
        public const int CHUNK_RADIUS = CHUNK_SIZE >> 1;
        public const int WORLD_HEIGHT_IN_CHUNKS = 256 / CHUNK_SIZE;
        public const int WORLD_HEIGHT = CHUNK_SIZE * WORLD_HEIGHT_IN_CHUNKS;

        public static int Seed { get; set; }
    }
}
