namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    public class WPF0051Benchmarks : AnalyzerBenchmarks
    {
        public WPF0051Benchmarks()
            : base(new WpfAnalyzers.WPF0051XmlnsDefinitionMustMapExistingNamespace())
        {
        }
    }
}
