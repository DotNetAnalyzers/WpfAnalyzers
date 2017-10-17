namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    public class WPF0030Benchmarks : AnalyzerBenchmarks
    {
        public WPF0030Benchmarks()
            : base(new WpfAnalyzers.WPF0030BackingFieldShouldBeStaticReadonly())
        {
        }
    }
}
