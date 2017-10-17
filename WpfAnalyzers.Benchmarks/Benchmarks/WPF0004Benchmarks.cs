namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    public class WPF0004Benchmarks : AnalyzerBenchmarks
    {
        public WPF0004Benchmarks()
            : base(new WpfAnalyzers.WPF0004ClrMethodShouldMatchRegisteredName())
        {
        }
    }
}
