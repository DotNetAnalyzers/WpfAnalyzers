namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    using BenchmarkDotNet.Attributes;
    using Microsoft.CodeAnalysis.Diagnostics;

    public abstract class AnalyzerBenchmarks
    {
        private readonly Gu.Roslyn.Asserts.Benchmark benchmark;

        protected AnalyzerBenchmarks(DiagnosticAnalyzer analyzer)
        {
            this.benchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, analyzer);
        }

        [Benchmark]
        public void RunOnWpfAnalyzers()
        {
            this.benchmark.Run();
        }
    }
}
