namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    public class WPF0052Benchmarks : AnalyzerBenchmarks
    {
        public WPF0052Benchmarks()
            : base(new WpfAnalyzers.WPF0052XmlnsDefinitionsDoesNotMapAllNamespaces())
        {
        }
    }
}
