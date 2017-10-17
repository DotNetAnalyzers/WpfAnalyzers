namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    public class WPF0006Benchmarks : AnalyzerBenchmarks
    {
        public WPF0006Benchmarks()
            : base(new WpfAnalyzers.WPF0006CoerceValueCallbackShouldMatchRegisteredName())
        {
        }
    }
}
