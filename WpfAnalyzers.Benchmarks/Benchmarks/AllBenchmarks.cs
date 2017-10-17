namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    using BenchmarkDotNet.Attributes;

    public class AllBenchmarks
    {
        //private static readonly Gu.Roslyn.Asserts.Benchmark INPC001 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new INPC001ImplementINotifyPropertyChanged());

        [Benchmark]
        public void INPC001ImplementINotifyPropertyChanged()
        {
            //INPC001.Run();
        }
    }
}