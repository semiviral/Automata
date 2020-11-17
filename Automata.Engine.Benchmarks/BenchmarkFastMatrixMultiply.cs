using System.Numerics;
using BenchmarkDotNet.Attributes;

namespace Automata.Engine.Benchmarks
{
    public class BenchmarkFastMatrixMultiply
    {
        public Matrix4x4 A;
        public Matrix4x4 B;

        [GlobalSetup]
        public void Setup()
        {
            A = Matrix4x4.Identity;
            B = Matrix4x4.CreateTranslation(5f, 5f, 5f);
        }

        [Benchmark]
        public Matrix4x4 RegularMultiply() => A * B;
    }
}
