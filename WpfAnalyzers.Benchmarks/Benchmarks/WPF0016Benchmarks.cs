namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    public class WPF0016Benchmarks
    {
        private static readonly Gu.Roslyn.Asserts.Benchmark Benchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new WpfAnalyzers.WPF0016DefaultValueIsSharedReferenceType());

        [BenchmarkDotNet.Attributes.Benchmark]
        public void RunOnPropertyChangedAnalyzers()
        {
            Benchmark.Run();
        }
    }
}
