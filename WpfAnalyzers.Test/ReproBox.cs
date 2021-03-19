namespace WpfAnalyzers.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    [Explicit]
    public static class ReproBox
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly IReadOnlyList<DiagnosticAnalyzer> AllAnalyzers =
            typeof(KnownSymbols).Assembly.GetTypes()
                               .Where(x => typeof(DiagnosticAnalyzer).IsAssignableFrom(x) && !x.IsAbstract)
                               .Select(t => (DiagnosticAnalyzer)Activator.CreateInstance(t))
                               .ToArray();

        private static readonly Solution Solution = CodeFactory.CreateSolution(
            new FileInfo("C:\\Git\\_GuOrg\\Gu.Wpf.DataGrid2D\\Gu.Wpf.DataGrid2D\\Gu.Wpf.DataGrid2D.csproj"),
            AllAnalyzers,
            MetadataReferences.FromAttributes());

        [TestCaseSource(nameof(AllAnalyzers))]
        public static void SolutionRepro(DiagnosticAnalyzer analyzer)
        {
            Assert.Inconclusive("VS does not understand [Explicit]");
            RoslynAssert.Valid(analyzer, Solution);
        }

        [TestCaseSource(nameof(AllAnalyzers))]
        public static void Repro(DiagnosticAnalyzer analyzer)
        {
            var code = @"
namespace N
{
    using System;
    using Window = System.Windows.Window;

    class C
    {
        [Obsolete]
        void M()
        {
        }
    }
}";

            RoslynAssert.Valid(analyzer, code);
        }
    }
}
