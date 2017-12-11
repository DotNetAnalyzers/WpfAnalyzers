﻿namespace WpfAnalyzers.Test.Documentation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class Tests
    {
        private static readonly IReadOnlyList<DescriptorInfo> Descriptors = typeof(AnalyzerCategory)
            .Assembly
            .GetTypes()
            .Where(t => typeof(DiagnosticAnalyzer).IsAssignableFrom(t))
            .Select(t => (DiagnosticAnalyzer)Activator.CreateInstance(t))
            .SelectMany(DescriptorInfo.Create)
            .ToArray();

        private static IReadOnlyList<DescriptorInfo> DescriptorsWithDocs => Descriptors.Where(d => d.DocExists).ToArray();

        private static string SolutionDirectory => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\");

        private static string DocumentsDirectory => Path.Combine(SolutionDirectory, "documentation");

        [TestCaseSource(nameof(Descriptors))]
        public void MissingDocs(DescriptorInfo descriptorInfo)
        {
            if (descriptorInfo.DocExists)
            {
                Assert.Pass();
            }

            var descriptor = descriptorInfo.Descriptor;
            var id = descriptor.Id;
            DumpIfDebug(CreateStub(descriptorInfo));
            File.WriteAllText(descriptorInfo.DocFileName + ".generated", CreateStub(descriptorInfo));
            Assert.Fail($"Documentation is missing for {id}");
        }

        [TestCaseSource(nameof(DescriptorsWithDocs))]
        public void TitleId(DescriptorInfo descriptorInfo)
        {
            Assert.AreEqual($"# {descriptorInfo.Descriptor.Id}", File.ReadLines(descriptorInfo.DocFileName).First());
        }

        [TestCaseSource(nameof(DescriptorsWithDocs))]
        public void Title(DescriptorInfo descriptorInfo)
        {
            Assert.AreEqual(File.ReadLines(descriptorInfo.DocFileName).Skip(1).First(), $"## {descriptorInfo.Descriptor.Title}");
        }

        [TestCaseSource(nameof(DescriptorsWithDocs))]
        public void Description(DescriptorInfo descriptorInfo)
        {
            var expected = File.ReadLines(descriptorInfo.DocFileName)
                               .SkipWhile(l => !l.StartsWith("## Description"))
                               .Skip(1)
                               .FirstOrDefault(l => !string.IsNullOrWhiteSpace(l));
            var actual = descriptorInfo.Descriptor.Description.ToString().Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).First();

            DumpIfDebug(expected);
            DumpIfDebug(actual);
            Assert.AreEqual(expected, actual);
        }

        [TestCaseSource(nameof(DescriptorsWithDocs))]
        public void Table(DescriptorInfo descriptorInfo)
        {
            var expected = GetTable(CreateStub(descriptorInfo));
            DumpIfDebug(expected);
            var actual = GetTable(File.ReadAllText(descriptorInfo.DocFileName));
            CodeAssert.AreEqual(expected, actual);
        }

        [TestCaseSource(nameof(DescriptorsWithDocs))]
        public void ConfigSeverity(DescriptorInfo descriptorInfo)
        {
            var expected = GetConfigSeverity(CreateStub(descriptorInfo));
            DumpIfDebug(expected);
            var actual = GetConfigSeverity(File.ReadAllText(descriptorInfo.DocFileName));
            CodeAssert.AreEqual(expected, actual);
        }

        [TestCaseSource(nameof(Descriptors))]
        public void UniqueIds(DescriptorInfo descriptorInfo)
        {
            Assert.AreEqual(1, Descriptors.Count(d => d.Descriptor.Id == descriptorInfo.Descriptor.Id));
        }

        [Test]
        public void Index()
        {
            var builder = new StringBuilder();
            builder.AppendLine("<!-- start generated table -->")
                   .AppendLine("<table>");
            foreach (var info in DescriptorsWithDocs.OrderBy(x => x.Descriptor.Id))
            {
                builder.AppendLine("<tr>");
                builder.AppendLine($@"  <td><a href=""{info.Descriptor.HelpLinkUri}"">{info.Descriptor.Id}</a></td>");
                builder.AppendLine($"  <td>{info.Descriptor.Title}</td>");
                builder.AppendLine("</tr>");
            }

            builder.AppendLine("<table>")
                   .Append("<!-- end generated table -->");
            var expected = builder.ToString();
            DumpIfDebug(expected);
            var actual = GetTable(File.ReadAllText(Path.Combine(SolutionDirectory, "Readme.md")));
            CodeAssert.AreEqual(expected, actual);
        }

        private static string CreateStub(DescriptorInfo descriptorInfo)
        {
            var descriptor = descriptorInfo.Descriptor;
            return CreateStub(
                descriptor.Id,
                descriptor.Title.ToString(),
                descriptor.DefaultSeverity,
                descriptor.IsEnabledByDefault,
                descriptorInfo.CodeFileUri,
                descriptor.Category,
                descriptorInfo.Analyzer.GetType().Name,
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
            private DescriptorInfo(DiagnosticAnalyzer analyzer, DiagnosticDescriptor descriptor)
            {
                this.Analyzer = analyzer;
                this.Descriptor = descriptor;
                this.DocFileName = Path.Combine(DocumentsDirectory, this.Descriptor.Id + ".md");
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

            public DiagnosticAnalyzer Analyzer { get; }

            public bool DocExists => File.Exists(this.DocFileName);

            public DiagnosticDescriptor Descriptor { get; }

            public string DocFileName { get; }

            public string CodeFileName { get; }

            public string CodeFileUri { get; }

            public static IEnumerable<DescriptorInfo> Create(DiagnosticAnalyzer analyzer)
            {
                foreach (var descriptor in analyzer.SupportedDiagnostics)
                {
                    yield return new DescriptorInfo(analyzer, descriptor);
                }
            }

            public override string ToString() => this.Descriptor.Id;
        }
    }
}