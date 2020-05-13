#region

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Automata;
using Automata.Collections;
using Automata.Noise;
using Automata.Numerics;
using Automata.Singletons;
using AutomataTest.Blocks;
using ComputeSharp;

#endregion

namespace AutomataTest.Chunks.Generation
{
    public class ChunkBuildingJob : ChunkTerrainJob
    {
        private static readonly ObjectPool<int[]> _HeightmapPool = new ObjectPool<int[]>();
        private static readonly ObjectPool<float[]> _CaveNoisePool = new ObjectPool<float[]>();

        private static readonly int[] _EmptyHeightmap = new int[0];
        private static readonly float[] _EmptyCavemap = new float[0];

        private int _NoiseSeedA;
        private int _NoiseSeedB;

        private float _Frequency;
        private float _Persistence;
        private TimeSpan _NoiseRetrievalTimeSpan;
        private TimeSpan _TerrainGenerationTimeSpan;

        private int[] _Heightmap;
        private float[] _Cavemap;

        public ChunkBuildingJob()
        {
            _Heightmap = _EmptyHeightmap;
            _Cavemap = _EmptyCavemap;
        }

        protected override async Task Process()
        {
            Stopwatch.Restart();

            GenerateNoise();

            Stopwatch.Stop();

            _NoiseRetrievalTimeSpan = Stopwatch.Elapsed;

            Stopwatch.Restart();

            _Blocks = new Octree<ushort>(GenerationConstants.CHUNK_SIZE, BlockRegistry.AirID, false);

            await BatchTasksAndAwaitAll().ConfigureAwait(false);

            Array.Clear(_Heightmap, 0, _Heightmap.Length);
            Array.Clear(_Cavemap, 0, _Cavemap.Length);

            _HeightmapPool.TryAdd(_Heightmap);
            _CaveNoisePool.TryAdd(_Cavemap);

            _Heightmap = _EmptyHeightmap;
            _Cavemap = _EmptyCavemap;

            Stopwatch.Stop();

            _TerrainGenerationTimeSpan = Stopwatch.Elapsed;
        }

        protected override Task ProcessIndex(int index)
        {
            GenerateIndex(index);

            return Task.CompletedTask;
        }

        protected override Task ProcessFinished()
        {
            if (!_CancellationToken.IsCancellationRequested)
            {
                Diagnostics.Instance["NoiseRetrieval"].Enqueue(_NoiseRetrievalTimeSpan);
                Diagnostics.Instance["TerrainGeneration"].Enqueue(_TerrainGenerationTimeSpan);
            }

            return Task.CompletedTask;
        }

        public void SetData(Vector3i originPoint, int seed, float frequency, float persistence)
        {
            SetData(originPoint);

            _NoiseSeedA = seed ^ 2;
            _NoiseSeedB = seed ^ 3;

            _Frequency = frequency;
            _Persistence = persistence;
        }

        public void ClearData()
        {
            _OriginPoint = default;
            _Frequency = default;
            _Persistence = default;
            _Blocks = default;
        }

        private void GenerateNoise()
        {
            _Heightmap = _HeightmapPool.Retrieve() ?? new int[GenerationConstants.CHUNK_SIZE_SQUARED];
            _Cavemap = _CaveNoisePool.Retrieve() ?? new float[GenerationConstants.CHUNK_SIZE_CUBED];

            using ReadWriteBuffer<int> heightmapBuffer = Gpu.Default.AllocateReadWriteBuffer<int>(GenerationConstants.CHUNK_SIZE_SQUARED);
            using ReadWriteBuffer<float> cavemapBuffer = Gpu.Default.AllocateReadWriteBuffer<float>(GenerationConstants.CHUNK_SIZE_CUBED);

            void NoiseKernel(ThreadIds threadIds)
            {
                if (threadIds.Y == 0)
                {
                    heightmapBuffer[threadIds.X + (GenerationConstants.CHUNK_SIZE * threadIds.Z)] = 128;
                }

                cavemapBuffer[threadIds.X + (GenerationConstants.CHUNK_SIZE * (threadIds.Z + (GenerationConstants.CHUNK_SIZE * threadIds.Y)))] = 0;
            }

            Gpu.Default.For(GenerationConstants.CHUNK_SIZE, GenerationConstants.CHUNK_SIZE, GenerationConstants.CHUNK_SIZE, NoiseKernel);

                heightmapBuffer.GetData(_Heightmap);
                cavemapBuffer.GetData(_Cavemap);
            }


            // for (int x = 0; x < GenerationConstants.CHUNK_SIZE; x++)
            // for (int z = 0; z < GenerationConstants.CHUNK_SIZE; z++)
            // {
            //     Vector2i xzCoords = new Vector2i(x, z);
            //     int heightmapIndex = Vector2i.Project1D(xzCoords, GenerationConstants.CHUNK_SIZE);
            //     _Heightmap[heightmapIndex] = GetHeightByGlobalPosition(new Vector2i(_OriginPoint.X, _OriginPoint.Z) + xzCoords);
            //
            //     for (int y = 0; y < GenerationConstants.CHUNK_SIZE; y++)
            //     {
            //         Vector3i localPosition = new Vector3i(x, y, z);
            //         Vector3i globalPosition = _OriginPoint + localPosition;
            //         int caveNoiseIndex = Vector3i.Project1D(localPosition, GenerationConstants.CHUNK_SIZE);
            //
            //         _Cavemap[caveNoiseIndex] = GetCaveNoiseByGlobalPosition(globalPosition);
            //     }
            // }


        private void GenerateIndex(int index)
        {
            Debug.Assert(_Blocks != null);
            Debug.Assert(_SeededRandom != null);

            Vector3i localPosition = Vector3i.Project3D(index, GenerationConstants.CHUNK_SIZE);
            int heightmapIndex = Vector2i.Project1D(new Vector2i(localPosition.X, localPosition.Z), GenerationConstants.CHUNK_SIZE);

            int noiseHeight = _Heightmap[heightmapIndex];

            if (noiseHeight < _OriginPoint.Y)
            {
                return;
            }

            int globalPositionY = _OriginPoint.Y + localPosition.Y;

            if ((globalPositionY < 4) && (globalPositionY <= _SeededRandom.Next(0, 4)))
            {
                _Blocks.SetPoint(localPosition, GetCachedBlockID("bedrock"));
                return;
            }
            else if (_Cavemap[index] < 0.000225f)
            {
                return;
            }

            if (globalPositionY == noiseHeight)
            {
                _Blocks.SetPoint(localPosition, GetCachedBlockID("grass"));
            }
            else if ((globalPositionY < noiseHeight) && (globalPositionY >= (noiseHeight - 3))) // lay dirt up to 3 blocks below noise height
            {
                _Blocks.SetPoint(localPosition, _SeededRandom.Next(0, 8) == 0
                    ? GetCachedBlockID("dirt_coarse")
                    : GetCachedBlockID("dirt"));
            }
            else if (globalPositionY < (noiseHeight - 3))
            {
                _Blocks.SetPoint(localPosition, _SeededRandom.Next(0, 100) == 0
                    ? GetCachedBlockID("coal_ore")
                    : GetCachedBlockID("stone"));
            }
        }

        private float GetCaveNoiseByGlobalPosition(Vector3i globalPosition)
        {
            float currentHeight = (globalPosition.Y + (((GenerationConstants.WORLD_HEIGHT / 4f) - (globalPosition.Y * 1.25f)) * _Persistence))
                                  * 0.85f;
            float heightDampener = AutomataMath.UnLerp(0f, GenerationConstants.WORLD_HEIGHT, currentHeight);
            float noiseA = OpenSimplexSlim.GetSimplex(_NoiseSeedA, 0.01f, globalPosition) * heightDampener;
            float noiseB = OpenSimplexSlim.GetSimplex(_NoiseSeedB, 0.01f, globalPosition) * heightDampener;
            float noiseAPow2 = (float)Math.Pow(noiseA, 2f);
            float noiseBPow2 = (float)Math.Pow(noiseB, 2f);

            return (noiseAPow2 + noiseBPow2) / 2f;
        }

        private int GetHeightByGlobalPosition(Vector2i globalPosition)
        {
            float noise = OpenSimplexSlim.GetSimplex(GenerationConstants.Seed, _Frequency, globalPosition);
            float noiseAsWorldHeight = AutomataMath.UnLerp(-1f, 1f, noise) * GenerationConstants.WORLD_HEIGHT;
            float noisePersistedWorldHeight =
                noiseAsWorldHeight + (((GenerationConstants.WORLD_HEIGHT / 2f) - (noiseAsWorldHeight * 1.25f)) * _Persistence);

            return (int)Hlsl.Floor(noisePersistedWorldHeight);
        }
    }
}
