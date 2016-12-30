﻿namespace WpfAnalyzers.Test.Documentation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;

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

        private static string DocumentsDirectory => Path.Combine(SolutionDirectory, "documentation");

        [TestCaseSource(nameof(Descriptors))]
        public void MissingDocs(DescriptorInfo descriptorInfo)
        {
            if (descriptorInfo.DocExists)
            {
                Assert.Pass();
            }

            var descriptor = descriptorInfo.DiagnosticDescriptor;
            var id = descriptor.Id;
            DumpIfDebug(CreateStub(descriptorInfo));
            File.WriteAllText(descriptorInfo.DocFileName + ".generated", CreateStub(descriptorInfo));
            Assert.Fail($"Documentation is missing for {id}");
        }

        [TestCaseSource(nameof(DescriptorsWithDocs))]
        public void TitleId(DescriptorInfo descriptorInfo)
        {
            Assert.AreEqual($"# {descriptorInfo.DiagnosticDescriptor.Id}", File.ReadLines(descriptorInfo.DocFileName).First());
        }

        [TestCaseSource(nameof(DescriptorsWithDocs))]
        public void Title(DescriptorInfo descriptorInfo)
        {
            Assert.AreEqual(File.ReadLines(descriptorInfo.DocFileName).Skip(1).First(), $"## {descriptorInfo.DiagnosticDescriptor.Title}");
        }

        [TestCaseSource(nameof(DescriptorsWithDocs))]
        public void Description(DescriptorInfo descriptorInfo)
        {
            var expected = File.ReadLines(descriptorInfo.DocFileName)
                               .SkipWhile(l => !l.StartsWith("## Description"))
                               .Skip(1)
                               .FirstOrDefault(l => !string.IsNullOrWhiteSpace(l));
            var actual = descriptorInfo.DiagnosticDescriptor.Description.ToString();

            DumpIfDebug(expected);
            DumpIfDebug(actual);
            Assert.AreEqual(expected, actual);
        }

        [TestCaseSource(nameof(DescriptorsWithDocs))]
        public void Table(DescriptorInfo descriptorInfo)
        {
            var expected = GetTable(CreateStub(descriptorInfo)).NormalizeNewLine();
            DumpIfDebug(expected);
            var actual = GetTable(File.ReadAllText(descriptorInfo.DocFileName)).NormalizeNewLine();
            Assert.AreEqual(expected, actual);
        }

        [TestCaseSource(nameof(DescriptorsWithDocs))]
        public void ConfigSeverity(DescriptorInfo descriptorInfo)
        {
            var expected = GetConfigSeverity(CreateStub(descriptorInfo)).NormalizeNewLine();
            DumpIfDebug(expected);
            var actual = GetConfigSeverity(File.ReadAllText(descriptorInfo.DocFileName)).NormalizeNewLine();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void UniqueIds()
        {
            CollectionAssert.AllItemsAreUnique(Descriptors.Select(x => x.DiagnosticDescriptor.Id));
            CollectionAssert.AllItemsAreUnique(Descriptors.Select(x => x.DiagnosticDescriptor.Title));
            CollectionAssert.AllItemsAreUnique(Descriptors.Select(x => x.DiagnosticDescriptor.Description));
        }

        [Test]
        public void Index()
        {
            var builder = new StringBuilder();
            builder.AppendLine("<!-- start generated table -->")
                   .AppendLine("<table>");
            foreach (var info in DescriptorsWithDocs.OrderBy(x => x.DiagnosticDescriptor.Id))
            {
                builder.AppendLine("<tr>");
                builder.AppendLine($@"  <td><a href=""{info.DiagnosticDescriptor.HelpLinkUri}"">{info.DiagnosticDescriptor.Id}</a></td>");
                builder.AppendLine($"  <td>{info.DiagnosticDescriptor.Title}</td>");
                builder.AppendLine("</tr>");
            }

            builder.AppendLine("<table>")
                   .Append("<!-- end generated table -->");
            var expected = builder.ToString().NormalizeNewLine();
            DumpIfDebug(expected);
            var actual = GetTable(File.ReadAllText(Path.Combine(SolutionDirectory, "Readme.md"))).NormalizeNewLine();
            Assert.AreEqual(expected, actual);
        }

        private static string CreateStub(DescriptorInfo descriptorInfo)
        {
            var descriptor = descriptorInfo.DiagnosticDescriptor;
            return CreateStub(
                descriptor.Id,
                descriptor.Title.ToString(),
                descriptor.DefaultSeverity,
                descriptor.IsEnabledByDefault,
                descriptorInfo.CodeFileUri,
                descriptor.Category,
                descriptorInfo.DiagnosticAnalyzer.GetType().Name,
                descriptor.Description.ToString());
        }

        private static string CreateStub(
            string id,
            string title,
            DiagnosticSeverity severity,
            bool enabled,
            string codeFileUrl,
            string category,
            string typeName,
            string description)
        {
            return Properties.Resources.DiagnosticDocTemplate.Replace("{ID}", id)
                             .Replace("## ADD TITLE HERE", $"## {title}")
                             .Replace("{SEVERITY}", severity.ToString())
                             .Replace("{ENABLED}", enabled ? "true" : "false")
                             .Replace("{CATEGORY}", category)
                             .Replace("{URL}", codeFileUrl ?? "https://github.com/DotNetAnalyzers/WpfAnalyzers")
                             .Replace("{TYPENAME}", typeName)
                             .Replace("ADD DESCRIPTION HERE", description ?? "ADD DESCRIPTION HERE")
                             .Replace("{TITLE}", title)
                             .Replace("{TRIMMEDTYPENAME}", typeName.Substring(id.Length));
        }

        private static string GetTable(string doc)
        {
            return GetSection(doc, "<!-- start generated table -->", "<!-- end generated table -->");
        }

        private static string GetConfigSeverity(string doc)
        {
            return GetSection(doc, "<!-- start generated config severity -->", "<!-- end generated config severity -->");
        }

        private static string GetSection(string doc, string startToken, string endToken)
        {
            var start = doc.IndexOf(startToken, StringComparison.Ordinal);
            var end = doc.IndexOf(endToken, StringComparison.Ordinal) + endToken.Length;
            return doc.Substring(start, end - start);
        }

        [Conditional("DEBUG")]
        private static void DumpIfDebug(string text)
        {
            Console.Write(text);
            Console.WriteLine();
            Console.WriteLine();
        }

        public class DescriptorInfo
        {
            public DescriptorInfo(DiagnosticAnalyzer analyzer)
            {
                this.DiagnosticAnalyzer = analyzer;
                this.DocFileName = Path.Combine(DocumentsDirectory, this.DiagnosticDescriptor.Id + ".md");
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