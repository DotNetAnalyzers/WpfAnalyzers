namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    public class WPF0002Benchmarks : AnalyzerBenchmarks
    {
        public WPF0002Benchmarks()
            : base(new WpfAnalyzers.WPF0002BackingFieldShouldMatchRegisteredName())
        {
        }
    }
}
