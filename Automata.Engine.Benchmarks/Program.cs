using System.Runtime.CompilerServices;
using Automata.Engine.Numerics;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace Automata.Engine.Benchmarks
{
    internal class Program
    {
        private static unsafe void Main(string[] args)
        {
            int a = -1;
            short b = Unsafe.Read<short>(&a);

            int value = Primitive<short>.Convert<int>(1);
            Summary summary = BenchmarkRunner.Run<BenchmarkVectorNumerics>();

            //Console.ReadKey();
        }
    }
}
