// ReSharper disable RedundantNameQualifier
namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    public class WPF0072Benchmarks
    {
        private static readonly Gu.Roslyn.Asserts.Benchmark Benchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.ValueConverterAnalyzer());

        [BenchmarkDotNet.Attributes.Benchmark]
        public void RunOnWpfValidCodeProject()
        {
            Benchmark.Run();
        }
    }
}
