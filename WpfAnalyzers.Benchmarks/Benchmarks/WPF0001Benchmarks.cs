namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    public class WPF0001Benchmarks : AnalyzerBenchmarks
    {
        public WPF0001Benchmarks()
            : base(new WpfAnalyzers.WPF0001BackingFieldShouldMatchRegisteredName())
        {
        }
    }
}
