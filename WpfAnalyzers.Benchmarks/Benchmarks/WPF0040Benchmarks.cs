namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    public class WPF0040Benchmarks
    {
        private static readonly Gu.Roslyn.Asserts.Benchmark Benchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new WpfAnalyzers.WPF0040SetUsingDependencyPropertyKey());

        [BenchmarkDotNet.Attributes.Benchmark]
        public void RunOnPropertyChangedAnalyzers()
        {
            Benchmark.Run();
        }
    }
}
