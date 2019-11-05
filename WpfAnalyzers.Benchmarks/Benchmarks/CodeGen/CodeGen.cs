namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    [Explicit("Script")]
    public static class CodeGen
    {
        private static IReadOnlyList<DiagnosticAnalyzer> AllAnalyzers { get; } = typeof(KnownSymbols).Assembly
                                                                                                    .GetTypes()
                                                                                                    .Where(typeof(DiagnosticAnalyzer).IsAssignableFrom)
                                                                                                    .Select(t => (DiagnosticAnalyzer)Activator.CreateInstance(t))
                                                                                                    .ToArray();

        [TestCaseSource(nameof(AllAnalyzers))]
        public static void AnalyzersBenchmark(DiagnosticAnalyzer analyzer)
        {
            var expectedName = analyzer.GetType().Name + "Benchmarks";
            var fileName = Path.Combine(Program.BenchmarksDirectory, expectedName + ".cs");
            var code = new StringBuilder().AppendLine("// ReSharper disable RedundantNameQualifier")
                                          .AppendLine($"namespace {typeof(AllBenchmarks).Namespace}")
                                          .AppendLine("{")
                                          .AppendLine("    [BenchmarkDotNet.Attributes.MemoryDiagnoser]")
                                          .AppendLine($"    public class {expectedName}")
                                          .AppendLine("    {")
                                          .AppendLine($"        private static readonly Gu.Roslyn.Asserts.Benchmark Benchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new {analyzer.GetType().FullName}());")
                                          .AppendLine()
                                          .AppendLine("        [BenchmarkDotNet.Attributes.Benchmark]")
                                          .AppendLine("        public void RunOnValidCodeProject()")
                                          .AppendLine("        {")
                                          .AppendLine("            Benchmark.Run();")
                                          .AppendLine("        }")
                                          .AppendLine("    }")
                                          .AppendLine("}")
                                          .ToString();
            if (!File.Exists(fileName) ||
                !CodeComparer.Equals(File.ReadAllText(fileName), code))
            {
                File.WriteAllText(fileName, code);
                Assert.Fail();
            }
        }

        [Test]
        public static void AllBenchmarks()
        {
            var fileName = Path.Combine(Program.BenchmarksDirectory, "AllBenchmarks.cs");
            var builder = new StringBuilder();
            builder.AppendLine("// ReSharper disable RedundantNameQualifier")
                   .AppendLine($"namespace {typeof(AllBenchmarks).Namespace}")
                   .AppendLine("{")
                   .AppendLine("    [BenchmarkDotNet.Attributes.MemoryDiagnoser]")
                   .AppendLine("    public class AllBenchmarks")
                   .AppendLine("    {");
            foreach (var analyzer in AllAnalyzers)
            {
                builder.AppendLine(
                           $"        private static readonly Gu.Roslyn.Asserts.Benchmark {analyzer.GetType().Name}Benchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new {analyzer.GetType().FullName}());")
                       .AppendLine();
            }

            foreach (var analyzer in AllAnalyzers)
            {
                builder.AppendLine($"        [BenchmarkDotNet.Attributes.Benchmark]")
                       .AppendLine($"        public void {analyzer.GetType().Name}()")
                       .AppendLine("        {")
                       .AppendLine($"            {analyzer.GetType().Name}Benchmark.Run();")
                       .AppendLine("        }");
                if (!ReferenceEquals(analyzer, AllAnalyzers[AllAnalyzers.Count - 1]))
                {
                    builder.AppendLine();
                }
            }

            builder.AppendLine("    }")
                   .AppendLine("}");

            var code = builder.ToString();
            if (!File.Exists(fileName) ||
                !CodeComparer.Equals(File.ReadAllText(fileName), code))
            {
                File.WriteAllText(fileName, code);
                Assert.Fail();
            }
        }

        [Test]
        public static void CopyResults()
        {
            foreach (var file in Directory.EnumerateFiles("C:\\Git\\_DotnetAnalyzers\\WpfAnalyzers\\WpfAnalyzers.Benchmarks\\bin\\Release\\net472\\BenchmarkDotNet.Artifacts\\results"))
            {
                File.Move(file, file.Replace("WpfAnalyzers.Benchmarks.Benchmarks.", string.Empty).Replace("-report-github", string.Empty));
            }
        }
    }
}
