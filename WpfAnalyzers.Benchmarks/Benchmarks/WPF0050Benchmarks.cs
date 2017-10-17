namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    public class WPF0050Benchmarks : AnalyzerBenchmarks
    {
        public WPF0050Benchmarks()
            : base(new WpfAnalyzers.WPF0050XmlnsPrefixMustMatchXmlnsDefinition())
        {
        }
    }
}
