// ReSharper disable RedundantNameQualifier
namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    public class WPF0052Benchmarks
    {
        private static readonly Gu.Roslyn.Asserts.Benchmark Benchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.WPF0052XmlnsDefinitionsDoesNotMapAllNamespaces());

        [BenchmarkDotNet.Attributes.Benchmark]
        public void RunOnWpfValidCodeProject()
        {
            Benchmark.Run();
        }
    }
}
