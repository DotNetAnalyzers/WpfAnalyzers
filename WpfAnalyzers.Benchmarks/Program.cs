// ReSharper disable UnusedMember.Local
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
        public static string BenchmarksDirectory { get; } = Path.Combine(ProjectFile.Find("WpfAnalyzers.Benchmarks.csproj").DirectoryName, "Benchmarks");

        public static void Main()
        {
            if (false)
            {
                var benchmark = Benchmark.Create(
                    Code.ValidCodeProject,
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
            var summaries = switcher.RunAll();
            return summaries;
        }

        private static IEnumerable<Summary> RunSingle<T>()
        {
            var summaries = new[] { BenchmarkRunner.Run<T>() };
            return summaries;
        }

        private static void CopyResult(Summary summary)
        {
            var name = summary.Title.Split('.').LastOrDefault()?.Split('-').FirstOrDefault();
            if (name == null)
            {
                Console.WriteLine("Did not find name in: " + summary.Title);
                Console.WriteLine("Press any key to exit.");
                _ = Console.ReadKey();
                return;
            }

            var pattern = $"{summary.Title.Split('-').First()}-report-github.md";
            var sourceFileName = Directory.EnumerateFiles(summary.ResultsDirectoryPath, pattern)
                                          .SingleOrDefault();
            if (sourceFileName == null)
            {
                Console.WriteLine("Did not find a file matching the pattern: " + pattern);
                Console.WriteLine("Press any key to exit.");
                _ = Console.ReadKey();
                return;
            }

            var destinationFileName = Path.ChangeExtension(FindCsFile(), ".md");
            Console.WriteLine($"Copy:");
            Console.WriteLine($"Source: {sourceFileName}");
            Console.WriteLine($"Target: {destinationFileName}");
            File.Copy(sourceFileName, destinationFileName, overwrite: true);

            string FindCsFile()
            {
                return Directory.EnumerateFiles(
                                    AppDomain.CurrentDomain.BaseDirectory.Split(new[] { "\\bin\\" }, StringSplitOptions.RemoveEmptyEntries).First(),
                                    $"{name}.cs",
                                    SearchOption.AllDirectories)
                                .Single();
            }
        }
    }
}
