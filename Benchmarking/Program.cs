using System;
using Automata.Engine.Extensions;
using Automata.Engine.Numerics;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace Benchmarking
{
    class Program
    {
        static void Main(string[] args)
        {
            Summary summary = BenchmarkRunner.Run<BenchmarkStructExtensions>();
        }
    }
}
