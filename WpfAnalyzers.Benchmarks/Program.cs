﻿// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local
// ReSharper disable HeuristicUnreachableCode
#pragma warning disable CS0162 // Unreachable code detected
#pragma warning disable GU0011 // Don't ignore the returnvalue.
namespace WpfAnalyzers.Benchmarks
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using BenchmarkDotNet.Reports;
    using BenchmarkDotNet.Running;
    using Gu.Roslyn.Asserts;
    using WpfAnalyzers.Benchmarks.Benchmarks;

    public class Program
    {
        public static string ProjectDirectory { get; } = CodeFactory.FindProjectFile("WpfAnalyzers.Benchmarks.csproj").DirectoryName;

        public static string BenchmarksDirectory { get; } = Path.Combine(ProjectDirectory, "Benchmarks");

        public static void Main()
        {
            if (false)
            {
                var benchmark = Gu.Roslyn.Asserts.Benchmark.Create(
                    Code.AnalyzersProject,
                    new SetValueAnalyzer());

                // Warmup
                benchmark.Run();
                Console.WriteLine("Attach profiler and press any key to continue...");
                Console.ReadKey();
                benchmark.Run();
            }
            else if (false)
            {
                foreach (var summary in RunSingle<AllBenchmarks>())
                {
                    CopyResult(summary);
                }
            }
            else
            {
                foreach (var summary in RunAll())
                {
                    CopyResult(summary);
                }
            }
        }

        private static IEnumerable<Summary> RunAll()
        {
            var switcher = new BenchmarkSwitcher(typeof(Program).Assembly);
            var summaries = switcher.Run(new[] { "*" });
            return summaries;
        }

        private static IEnumerable<Summary> RunSingle<T>()
        {
            var summaries = new[] { BenchmarkRunner.Run<T>() };
            return summaries;
        }

        private static void CopyResult(Summary summary)
        {
            Console.WriteLine($"DestinationDirectory: {BenchmarksDirectory}");
            if (Directory.Exists(BenchmarksDirectory))
            {
                var sourceFileName = Directory.EnumerateFiles(summary.ResultsDirectoryPath)
                                              .Single(x => x.EndsWith(summary.Title + "-report-github.md"));
                var destinationFileName = Path.Combine(BenchmarksDirectory, summary.Title + ".md");
                Console.WriteLine($"Copy: {sourceFileName} -> {destinationFileName}");
                File.Copy(sourceFileName, destinationFileName, overwrite: true);
            }
        }
    }
}
