namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    public class WPF0032Benchmarks : AnalyzerBenchmarks
    {
        public WPF0032Benchmarks()
            : base(new WpfAnalyzers.WPF0032ClrPropertyGetAndSetSameDependencyProperty())
        {
        }
    }
}
