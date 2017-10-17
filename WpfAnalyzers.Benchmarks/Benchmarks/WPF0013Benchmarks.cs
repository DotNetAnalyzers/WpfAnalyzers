namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    public class WPF0013Benchmarks : AnalyzerBenchmarks
    {
        public WPF0013Benchmarks()
            : base(new WpfAnalyzers.WPF0013ClrMethodMustMatchRegisteredType())
        {
        }
    }
}
