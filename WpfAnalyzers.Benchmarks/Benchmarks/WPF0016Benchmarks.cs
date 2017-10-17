namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    public class WPF0016Benchmarks : AnalyzerBenchmarks
    {
        public WPF0016Benchmarks()
            : base(new WpfAnalyzers.WPF0016DefaultValueIsSharedReferenceType())
        {
        }
    }
}
