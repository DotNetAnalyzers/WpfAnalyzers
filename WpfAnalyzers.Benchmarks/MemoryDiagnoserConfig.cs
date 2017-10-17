[assembly: BenchmarkDotNet.Attributes.Config(typeof(WpfAnalyzers.Benchmarks.MemoryDiagnoserConfig))]
namespace WpfAnalyzers.Benchmarks
{
    using BenchmarkDotNet.Configs;
    using BenchmarkDotNet.Diagnosers;

    public class MemoryDiagnoserConfig : ManualConfig
    {
        public MemoryDiagnoserConfig()
        {
            this.Add(new MemoryDiagnoser());
        }
    }
}