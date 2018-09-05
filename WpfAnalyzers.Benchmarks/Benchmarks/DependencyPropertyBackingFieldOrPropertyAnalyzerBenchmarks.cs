// ReSharper disable RedundantNameQualifier
namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    public class DependencyPropertyBackingFieldOrPropertyAnalyzerBenchmarks
    {
        private static readonly Gu.Roslyn.Asserts.Benchmark Benchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.DependencyPropertyBackingFieldOrPropertyAnalyzer());

        [BenchmarkDotNet.Attributes.Benchmark]
        public void RunOnValidCodeProject()
        {
            Benchmark.Run();
        }
    }
}
