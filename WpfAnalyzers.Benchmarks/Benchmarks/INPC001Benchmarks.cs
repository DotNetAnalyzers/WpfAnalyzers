namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    public class INPC001Benchmarks : AnalyzerBenchmarks
    {
        public INPC001Benchmarks()
            : base(new WpfAnalyzers.WPF0001BackingFieldShouldMatchRegisteredName())
        {
        }
    }
}