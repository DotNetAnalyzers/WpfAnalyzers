namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    public class WPF0014Benchmarks : AnalyzerBenchmarks
    {
        public WPF0014Benchmarks()
            : base(new WpfAnalyzers.WPF0014SetValueMustUseRegisteredType())
        {
        }
    }
}
