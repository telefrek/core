using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Newtonsoft.Json.Linq;

namespace Telefrek.Core.Json.Benchmark
{
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [MemoryDiagnoser]
    [ThreadingDiagnoser]
    public class SimpleSerializationBenchmark
    {
        private static readonly string TEST_JSON = System.IO.File.ReadAllText("test.json");

        [Benchmark(Description = "Telefre.Core.Json")]
        public void Telefrek()
        {
            var element = TEST_JSON.AsJson();
        }

        [Benchmark(Description = "dotNET.Json")]
        public void Dotnet()
        {
            var element = System.Text.Json.JsonDocument.Parse(TEST_JSON);
        }

        [Benchmark(Baseline = true, Description = "Json.NET")]
        public void JsonNet()
        {
            var element = JObject.Parse(TEST_JSON);
        }
    }
}