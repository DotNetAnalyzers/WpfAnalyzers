namespace WpfAnalyzers.Benchmarks
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;
    using WpfAnalyzers.Benchmarks.Benchmarks;

    internal class BenchmarkTests
    {
        private static IReadOnlyList<DiagnosticAnalyzer> AllAnalyzers { get; } = typeof(KnownSymbol).Assembly
                                                                                                    .GetTypes()
                                                                                                    .Where(typeof(DiagnosticAnalyzer).IsAssignableFrom)
                                                                                                    .Select(t => (DiagnosticAnalyzer)Activator.CreateInstance(t))
                                                                                                    .ToArray();

        private static IReadOnlyList<Type> AllBenchmarkTypes { get; } = typeof(AnalyzerBenchmarks).Assembly.GetTypes()
                                                                                                  .Where(typeof(AnalyzerBenchmarks).IsAssignableFrom)
                                                                                                  .ToArray();

        private static IReadOnlyList<Gu.Roslyn.Asserts.Benchmark> AllBenchmars { get; } = AllAnalyzers
            .Select(x => Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, x))
            .ToArray();

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            foreach (var walker in AllBenchmars)
            {
                walker.Run();
            }
        }

        [TestCaseSource(nameof(AllBenchmars))]
        public void Run(Gu.Roslyn.Asserts.Benchmark walker)
        {
            walker.Run();
        }

        [Test]
        public void BenchmarksDirectoryExists()
        {
            Assert.AreEqual(true, Directory.Exists(Program.BenchmarksDirectory), Program.BenchmarksDirectory);
        }
    }
}