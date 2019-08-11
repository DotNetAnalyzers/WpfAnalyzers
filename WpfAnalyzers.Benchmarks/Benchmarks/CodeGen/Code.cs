namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;

    public static class Code
    {
        private static readonly IReadOnlyList<DiagnosticAnalyzer> AllAnalyzers = typeof(KnownSymbol).Assembly
                                                                                                    .GetTypes()
                                                                                                    .Where(typeof(DiagnosticAnalyzer).IsAssignableFrom)
                                                                                                    .Select(t => (DiagnosticAnalyzer)Activator.CreateInstance(t))
                                                                                                    .ToArray();

        public static Solution ValidCodeProject { get; } = CodeFactory.CreateSolution(
            ProjectFile.Find("ValidCode.csproj"),
            AllAnalyzers,
            MetadataReferences.Transitive(typeof(System.Windows.Window)));
    }
}
