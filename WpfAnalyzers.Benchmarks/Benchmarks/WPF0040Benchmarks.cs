namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    public class WPF0040Benchmarks : AnalyzerBenchmarks
    {
        public WPF0040Benchmarks()
            : base(new WpfAnalyzers.WPF0040SetUsingDependencyPropertyKey())
        {
        }
    }
}
