namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    public class WPF0003Benchmarks : AnalyzerBenchmarks
    {
        public WPF0003Benchmarks()
            : base(new WpfAnalyzers.WPF0003ClrPropertyShouldMatchRegisteredName())
        {
        }
    }
}
