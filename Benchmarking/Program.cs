using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace Benchmarking
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Summary summary = BenchmarkRunner.Run<BenchmarkStructExtensions>();
        }
    }
}
