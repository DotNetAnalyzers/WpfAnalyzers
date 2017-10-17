namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    public class WPF0043Benchmarks : AnalyzerBenchmarks
    {
        public WPF0043Benchmarks()
            : base(new WpfAnalyzers.WPF0043DontUseSetCurrentValueForDataContext())
        {
        }
    }
}
