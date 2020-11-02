namespace WpfAnalyzers.Benchmarks
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;
    using WpfAnalyzers.Benchmarks.Benchmarks;

    [Ignore("Broken By Roslyn 3.5.0")]
    public static class BenchmarkTests
    {
        private static IReadOnlyList<DiagnosticAnalyzer> AllAnalyzers { get; } = typeof(KnownSymbols).Assembly
                                                                                                    .GetTypes()
                                                                                                    .Where(typeof(DiagnosticAnalyzer).IsAssignableFrom)
                                                                                                    .Select(t => (DiagnosticAnalyzer)Activator.CreateInstance(t))
                                                                                                    .ToArray();

        private static IReadOnlyList<Gu.Roslyn.Asserts.Benchmark> AllBenchmarks { get; } = AllAnalyzers
            .Select(x => Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, x))
            .ToArray();

        [OneTimeSetUp]
        public static void OneTimeSetUp()
        {
            foreach (var benchmark in AllBenchmarks)
            {
                benchmark.Run();
            }
        }

        [TestCaseSource(nameof(AllBenchmarks))]
        public static void Run(Gu.Roslyn.Asserts.Benchmark benchmark)
        {
            benchmark.Run();
        }

        [Test]
        public static void BenchmarksDirectoryExists()
        {
            Assert.AreEqual(true, Directory.Exists(Program.BenchmarksDirectory), Program.BenchmarksDirectory);
        }
    }
}
