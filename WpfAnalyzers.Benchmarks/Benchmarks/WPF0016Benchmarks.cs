// ReSharper disable RedundantNameQualifier
namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    public class WPF0016Benchmarks
    {
        private static readonly Gu.Roslyn.Asserts.Benchmark Benchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new WpfAnalyzers.PropertyMetadataAnalyzer());

        [BenchmarkDotNet.Attributes.Benchmark]
        public void RunOnWpfAnalyzersProject()
        {
            Benchmark.Run();
        }
    }
}
