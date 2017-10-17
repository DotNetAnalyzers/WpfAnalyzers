namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    public class WPF0012Benchmarks : AnalyzerBenchmarks
    {
        public WPF0012Benchmarks()
            : base(new WpfAnalyzers.WPF0012ClrPropertyShouldMatchRegisteredType())
        {
        }
    }
}
