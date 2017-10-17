namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    public class WPF0015Benchmarks : AnalyzerBenchmarks
    {
        public WPF0015Benchmarks()
            : base(new WpfAnalyzers.WPF0015RegisteredOwnerTypeMustBeDependencyObject())
        {
        }
    }
}
