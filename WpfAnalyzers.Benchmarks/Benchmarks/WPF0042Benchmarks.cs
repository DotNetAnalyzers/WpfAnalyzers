namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    public class WPF0042Benchmarks : AnalyzerBenchmarks
    {
        public WPF0042Benchmarks()
            : base(new WpfAnalyzers.WPF0042AvoidSideEffectsInClrAccessors())
        {
        }
    }
}
