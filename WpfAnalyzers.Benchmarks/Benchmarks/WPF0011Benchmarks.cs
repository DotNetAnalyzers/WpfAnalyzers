// ReSharper disable RedundantNameQualifier
namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    public class WPF0011Benchmarks
    {
        private static readonly Gu.Roslyn.Asserts.Benchmark Benchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.WPF0011ContainingTypeShouldBeRegisteredOwner());

        [BenchmarkDotNet.Attributes.Benchmark]
        public void RunOnWpfValidCodeProject()
        {
            Benchmark.Run();
        }
    }
}
