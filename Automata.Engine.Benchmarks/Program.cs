using System;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace Automata.Engine.Benchmarks
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Summary summary = BenchmarkRunner.Run<BenchmarkChecks>();
            Console.ReadKey();
        }
    }
}
