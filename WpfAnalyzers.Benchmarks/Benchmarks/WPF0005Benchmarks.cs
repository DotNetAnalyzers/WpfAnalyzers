namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    public class WPF0005Benchmarks : AnalyzerBenchmarks
    {
        public WPF0005Benchmarks()
            : base(new WpfAnalyzers.WPF0005PropertyChangedCallbackShouldMatchRegisteredName())
        {
        }
    }
}
