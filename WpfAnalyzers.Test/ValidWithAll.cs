﻿namespace WpfAnalyzers.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;

    using NUnit.Framework;

    public static class ValidWithAll
    {
        private static readonly IReadOnlyList<DiagnosticAnalyzer> AllAnalyzers = typeof(KnownSymbols)
            .Assembly.GetTypes()
            .Where(x => typeof(DiagnosticAnalyzer).IsAssignableFrom(x) && !x.IsAbstract)
            .Select(t => (DiagnosticAnalyzer)Activator.CreateInstance(t))
            .ToArray();

        private static readonly Solution AnalyzerProjectSolution = CodeFactory.CreateSolution(
            ProjectFile.Find("WpfAnalyzers.csproj"),
            AllAnalyzers,
            MetadataReferences.FromAttributes());

        private static readonly Solution ValidCodeProjectSln = CodeFactory.CreateSolution(
            ProjectFile.Find("ValidCode.csproj"),
            AllAnalyzers,
            MetadataReferences.FromAttributes());

        [Test]
        public static void NotEmpty()
        {
            CollectionAssert.IsNotEmpty(AllAnalyzers);
            Assert.Pass($"Count: {AllAnalyzers.Count}");
        }

        [TestCaseSource(nameof(AllAnalyzers))]
        public static void ValidCodeProject(DiagnosticAnalyzer analyzer)
        {
            RoslynAssert.Valid(analyzer, ValidCodeProjectSln);
        }

        [Ignore("Not working with nullable attributes from RAA")]
        [TestCaseSource(nameof(AllAnalyzers))]
        public static void AnalyzerProject(DiagnosticAnalyzer analyzer)
        {
            RoslynAssert.Valid(analyzer, AnalyzerProjectSolution);
        }
    }
}
