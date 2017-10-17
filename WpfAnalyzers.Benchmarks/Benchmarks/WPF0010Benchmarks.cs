namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    public class WPF0010Benchmarks : AnalyzerBenchmarks
    {
        public WPF0010Benchmarks()
            : base(new WpfAnalyzers.WPF0010DefaultValueMustMatchRegisteredType())
        {
        }
    }
}
