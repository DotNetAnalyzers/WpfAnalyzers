namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    public class WPF0041Benchmarks : AnalyzerBenchmarks
    {
        public WPF0041Benchmarks()
            : base(new WpfAnalyzers.WPF0041SetMutableUsingSetCurrentValue())
        {
        }
    }
}
