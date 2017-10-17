namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    public class WPF0007Benchmarks : AnalyzerBenchmarks
    {
        public WPF0007Benchmarks()
            : base(new WpfAnalyzers.WPF0007ValidateValueCallbackCallbackShouldMatchRegisteredName())
        {
        }
    }
}
