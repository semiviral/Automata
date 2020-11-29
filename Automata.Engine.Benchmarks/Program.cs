using Automata.Engine.Numerics;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace Automata.Engine.Benchmarks
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Vector2<float> a = new Vector2<float>(2f);
            Vector2<float> aa = new Vector2<float>(3f);
            Vector2<float> b = new Vector2<float>(2f);
            bool equal = a == b;
            Summary summary = BenchmarkRunner.Run<BenchmarkVectorNumerics>();

            //Console.ReadKey();
        }
    }
}