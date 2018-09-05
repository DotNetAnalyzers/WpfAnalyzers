// ReSharper disable RedundantNameQualifier
namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    public class WPF0105Benchmarks
    {
        private static readonly Gu.Roslyn.Asserts.Benchmark Benchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.RoutedEventEventDeclarationAnalyzer());

        [BenchmarkDotNet.Attributes.Benchmark]
        public void RunOnWpfValidCodeProject()
        {
            Benchmark.Run();
        }
    }
}
