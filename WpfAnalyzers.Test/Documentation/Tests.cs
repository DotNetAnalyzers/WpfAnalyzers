namespace WpfAnalyzers.Test.Documentation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class Tests
    {
        private static readonly IReadOnlyList<DescriptorInfo> Descriptors =
            typeof(AnalyzerCategory).Assembly.GetTypes()
                                    .Where(t => typeof(DiagnosticAnalyzer).IsAssignableFrom(t))
                                    .Select(t => (DiagnosticAnalyzer)Activator.CreateInstance(t))
                                    .Select(DescriptorInfo.Create)
                                    .ToArray();
        private static IReadOnlyList<DescriptorInfo> DescriptorsWithDocs => Descriptors.Where(d => d.DocExists).ToArray();

        private static string SolutionDirectory => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\");

        private static string DocumentssDirectory => Path.Combine(SolutionDirectory, "documentation");

        [TestCaseSource(nameof(Descriptors))]
        public void MissingDocs(DescriptorInfo descriptorInfo)
        {
            if (descriptorInfo.DocExists)
            {
                Assert.Pass();
            }

            var descriptor = descriptorInfo.DiagnosticDescriptor;
            var id = descriptor.Id;
            var doc = Properties.Resources.DiagnosticDocTemplate
                                .Replace("{ID}", id)
                                .Replace("## ADD TITLE HERE", $"## {descriptor.Title}")
                                .Replace("{SEVERITY}", descriptor.DefaultSeverity.ToString())
                                .Replace("{CATEGORY}", descriptor.Category)
                                .Replace("{URL}", descriptorInfo.CodeFileUri)
                                .Replace("{TYPENAME}", descriptorInfo.DiagnosticAnalyzer.GetType().Name)
                                .Replace("ADD DESCRIPTION HERE", descriptor.Description.ToString())
                                .Replace("{TITLE}", descriptorInfo.DiagnosticDescriptor.Title.ToString())
                                .Replace("{TRIMMEDTYPENAME}", descriptorInfo.DiagnosticAnalyzer.GetType().Name.Substring(id.Length));
            DumpIfDebug(doc);
            ////File.WriteAllText(descriptorInfo.DocFileName, doc);
            Assert.Inconclusive($"Documentation is missing for {id}");
        }

        [TestCaseSource(nameof(DescriptorsWithDocs))]
        public void TitleId(DescriptorInfo descriptorInfo)
        {
            Assert.AreEqual($"# {descriptorInfo.DiagnosticDescriptor.Id}", File.ReadLines(descriptorInfo.DocFileName).First());
        }

        [TestCaseSource(nameof(DescriptorsWithDocs))]
        public void Title(DescriptorInfo descriptorInfo)
        {
            Assert.AreEqual($"## {descriptorInfo.DiagnosticDescriptor.Title}", File.ReadLines(descriptorInfo.DocFileName).Skip(1).First());
        }

        [TestCaseSource(nameof(DescriptorsWithDocs))]
        public void TableId(DescriptorInfo descriptorInfo)
        {
            Assert.AreEqual($"  <td>{descriptorInfo.DiagnosticDescriptor.Id}</td>", File.ReadLines(descriptorInfo.DocFileName).SkipWhile(l=>l!= "<!-- start generated table -->").Skip(4).First());
        }

        [TestCaseSource(nameof(DescriptorsWithDocs))]
        public void TableSeverity(DescriptorInfo descriptorInfo)
        {
            Assert.AreEqual($"  <td>{descriptorInfo.DiagnosticDescriptor.DefaultSeverity}</td>", File.ReadLines(descriptorInfo.DocFileName).SkipWhile(l => l != "<!-- start generated table -->").Skip(8).First());
        }

        [TestCaseSource(nameof(DescriptorsWithDocs))]
        public void TableCategory(DescriptorInfo descriptorInfo)
        {
            Assert.AreEqual($"  <td>{descriptorInfo.DiagnosticDescriptor.Category}</td>", File.ReadLines(descriptorInfo.DocFileName).SkipWhile(l => l != "<!-- start generated table -->").Skip(12).First());
        }

        [TestCaseSource(nameof(DescriptorsWithDocs))]
        public void TableLink(DescriptorInfo descriptorInfo)
        {
            var expected = FormatLinkRow(descriptorInfo);
            DumpIfDebug(expected);
            Assert.AreEqual(expected, File.ReadLines(descriptorInfo.DocFileName).SkipWhile(l => l != "<!-- start generated table -->").Skip(16).First());
        }

        [Conditional("DEBUG")]
        private static void DumpIfDebug(string text)
        {
            Console.Write(text);
            Console.WriteLine();
            Console.WriteLine();
        }

        private static string FormatLinkRow(DescriptorInfo descriptorInfo)
        {
            return $@"  <td><a href=""{descriptorInfo.CodeFileUri}"">{descriptorInfo.DiagnosticAnalyzer.GetType().Name}</a></td>";
        }

        public class DescriptorInfo
        {
            public DescriptorInfo(DiagnosticAnalyzer analyzer)
            {
                this.DiagnosticAnalyzer = analyzer;
                this.DocFileName = Path.Combine(DocumentssDirectory, this.DiagnosticDescriptor.Id + ".md");
                this.CodeFileName = Directory.EnumerateFiles(
                                                 SolutionDirectory,
                                                 analyzer.GetType().Name + ".cs",
                                                 SearchOption.AllDirectories)
                                             .FirstOrDefault();
                this.CodeFileUri = this.CodeFileName != null
                    ? @"https://github.com/DotNetAnalyzers/WpfAnalyzers/blob/master/" +
                      this.CodeFileName.Substring(SolutionDirectory.Length).Replace("\\", "/")
                    : "missing";
            }

            public DiagnosticAnalyzer DiagnosticAnalyzer { get; }

            public bool DocExists => File.Exists(this.DocFileName);

            public DiagnosticDescriptor DiagnosticDescriptor => this.DiagnosticAnalyzer.SupportedDiagnostics.Single();

            public string DocFileName { get; }

            public string CodeFileName { get; }

            public string CodeFileUri { get; }

            public static DescriptorInfo Create(DiagnosticAnalyzer analyzer) => new DescriptorInfo(analyzer);

            public override string ToString() => this.DiagnosticDescriptor.Id;
        }
    }
}
