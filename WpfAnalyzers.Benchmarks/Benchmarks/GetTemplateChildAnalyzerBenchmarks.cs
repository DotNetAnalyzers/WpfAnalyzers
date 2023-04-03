// ReSharper disable RedundantNameQualifier
namespace WpfAnalyzers.Benchmarks.Benchmarks;

[BenchmarkDotNet.Attributes.MemoryDiagnoser]
public class GetTemplateChildAnalyzerBenchmarks
{
    private static readonly Gu.Roslyn.Asserts.Benchmark Benchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.GetTemplateChildAnalyzer());

    [BenchmarkDotNet.Attributes.Benchmark]
    public void RunOnValidCodeProject()
    {
        Benchmark.Run();
    }
}
