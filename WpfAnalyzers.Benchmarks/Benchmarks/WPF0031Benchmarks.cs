namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    public class WPF0031Benchmarks : AnalyzerBenchmarks
    {
        public WPF0031Benchmarks()
            : base(new WpfAnalyzers.WPF0031FieldOrder())
        {
        }
    }
}
