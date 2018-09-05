// ReSharper disable RedundantNameQualifier
namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    public class WPF0014Benchmarks
    {
        private static readonly Gu.Roslyn.Asserts.Benchmark Benchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.SetValueAnalyzer());

        [BenchmarkDotNet.Attributes.Benchmark]
        public void RunOnWpfValidCodeProject()
        {
            Benchmark.Run();
        }
    }
}
