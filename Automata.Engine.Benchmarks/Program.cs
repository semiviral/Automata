using System;
using System.Linq;
using Automata.Engine.Extensions;
using Automata.Engine.Numerics;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace Automata.Engine.Benchmarks
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Summary summary = BenchmarkRunner.Run<BenchmarkVectorNumerics>();

            //Console.ReadKey();
        }
    }
}