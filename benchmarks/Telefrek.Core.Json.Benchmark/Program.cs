using System;
using BenchmarkDotNet.Running;

namespace Telefrek.Core.Json.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
