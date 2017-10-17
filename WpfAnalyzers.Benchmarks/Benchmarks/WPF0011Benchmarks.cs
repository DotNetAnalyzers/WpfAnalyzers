namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    public class WPF0011Benchmarks : AnalyzerBenchmarks
    {
        public WPF0011Benchmarks()
            : base(new WpfAnalyzers.WPF0011ContainingTypeShouldBeRegisteredOwner())
        {
        }
    }
}
