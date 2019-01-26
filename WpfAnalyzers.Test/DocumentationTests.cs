#pragma warning disable CA1055 // Uri return values should not be strings
#pragma warning disable CA1056 // Uri properties should not be strings
#pragma warning disable CA1721 // Property names should not match get methods
namespace WpfAnalyzers.Test
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class DocumentationTests
    {
        private static readonly IReadOnlyList<DiagnosticAnalyzer> Analyzers = typeof(AnalyzerCategory)
                                                                              .Assembly
                                                                              .GetTypes()
                                                                              .Where(t => typeof(DiagnosticAnalyzer).IsAssignableFrom(t))
                                                                              .OrderBy(x => x.Name)
                                                                              .Select(t => (DiagnosticAnalyzer)Activator.CreateInstance(t))
                                                                              .ToArray();

        private static readonly IReadOnlyList<DescriptorInfo> DescriptorInfos = Analyzers
                                                                                .SelectMany(DescriptorInfo.Create)
                                                                                .ToArray();

        private static IReadOnlyList<DescriptorInfo> DescriptorsWithDocs => DescriptorInfos.Where(d => d.DocumentationFile.Exists)
                                                                                           .ToArray();

        private static DirectoryInfo SolutionDirectory => SolutionFile.Find("WpfAnalyzers.sln")
                                                                      .Directory;

        private static DirectoryInfo DocumentsDirectory => SolutionDirectory.EnumerateDirectories("documentation", SearchOption.TopDirectoryOnly)
                                                                            .Single();

        [TestCaseSource(nameof(DescriptorInfos))]
        public void MissingDocs(DescriptorInfo descriptorInfo)
        {
            if (!descriptorInfo.DocumentationFile.Exists)
            {
                var descriptor = descriptorInfo.Descriptor;
                var id = descriptor.Id;
                DumpIfDebug(CreateStub(descriptorInfo));
                File.WriteAllText(descriptorInfo.DocumentationFile.Name + ".generated", CreateStub(descriptorInfo));
                Assert.Fail($"Documentation is missing for {id}");
            }
        }

        [TestCaseSource(nameof(DescriptorsWithDocs))]
        public void TitleId(DescriptorInfo descriptorInfo)
        {
            Assert.AreEqual($"# {descriptorInfo.Descriptor.Id}", descriptorInfo.DocumentationFile.AllLines[0]);
        }

        [TestCaseSource(nameof(DescriptorsWithDocs))]
        public void Title(DescriptorInfo descriptorInfo)
        {
            var expected = $"## {descriptorInfo.Descriptor.Title}";
            var actual = descriptorInfo.DocumentationFile.AllLines
                                       .Skip(1)
                                       .First()
                                       .Replace("`", string.Empty);
            Assert.AreEqual(expected, actual);
        }

        [TestCaseSource(nameof(DescriptorsWithDocs))]
        public void Description(DescriptorInfo descriptorInfo)
        {
            var expected = descriptorInfo.Descriptor
                                         .Description
                                         .ToString(CultureInfo.InvariantCulture)
                                         .Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                                         .First();
            var actual = descriptorInfo.DocumentationFile.AllLines
                                       .SkipWhile(l => !l.StartsWith("## Description", StringComparison.OrdinalIgnoreCase))
                                       .Skip(1)
                                       .FirstOrDefault(l => !string.IsNullOrWhiteSpace(l))
                                       ?.Replace("`", string.Empty);

            DumpIfDebug(expected);
            DumpIfDebug(actual);
            Assert.AreEqual(expected, actual);
        }

        [TestCaseSource(nameof(DescriptorsWithDocs))]
        public void Table(DescriptorInfo descriptorInfo)
        {
            var expected = GetTable(CreateStub(descriptorInfo));
            DumpIfDebug(expected);
            var actual = GetTable(descriptorInfo.DocumentationFile.AllText);
            CodeAssert.AreEqual(expected, actual);
        }

        [TestCaseSource(nameof(DescriptorsWithDocs))]
        public void ConfigSeverity(DescriptorInfo descriptorInfo)
        {
            var expected = GetConfigSeverity(CreateStub(descriptorInfo));
            DumpIfDebug(expected);
            var actual = GetConfigSeverity(descriptorInfo.DocumentationFile.AllText);
            CodeAssert.AreEqual(expected, actual);
        }

        [TestCaseSource(nameof(DescriptorInfos))]
        public void UniqueIds(DescriptorInfo descriptorInfo)
        {
            Assert.AreEqual(1, DescriptorInfos.Select(x => x.Descriptor)
                                              .Distinct()
                                              .Count(d => d.Id == descriptorInfo.Descriptor.Id));
        }

        [Test]
        public void Index()
        {
            var builder = new StringBuilder();
            builder.AppendLine("<!-- start generated table -->")
                   .AppendLine("<table>");
            foreach (var descriptor in DescriptorsWithDocs.Select(x => x.Descriptor)
                                                          .Distinct()
                                                          .OrderBy(x => x.Id))
            {
                builder.AppendLine("  <tr>")
                       .AppendLine($@"    <td><a href=""{descriptor.HelpLinkUri}"">{descriptor.Id}</a></td>")
                       .AppendLine($"    <td>{descriptor.Title}</td>")
                       .AppendLine("  </tr>");
            }

            builder.AppendLine("<table>")
                   .Append("<!-- end generated table -->");
            var expected = builder.ToString();
            DumpIfDebug(expected);
            var actual = GetTable(File.ReadAllText(Path.Combine(SolutionDirectory.FullName, "Readme.md")));
            CodeAssert.AreEqual(expected, actual);
        }

        private static string CreateStub(DescriptorInfo descriptorInfo)
        {
            var descriptor = descriptorInfo.Descriptor;
            var stub = $@"# {descriptor.Id}
## {descriptor.Title.ToString(CultureInfo.InvariantCulture)}

<!-- start generated table -->
<table>
  <tr>
    <td>CheckId</td>
    <td>{descriptor.Id}</td>
  </tr>
  <tr>
    <td>Severity</td>
    <td>{descriptor.DefaultSeverity.ToString()}</td>
  </tr>
  <tr>
    <td>Enabled</td>
    <td>{(descriptor.IsEnabledByDefault ? "True" : "False")}</td>
  </tr>
  <tr>
    <td>Category</td>
    <td>{descriptor.Category}</td>
  </tr>
  <tr>
    <td>Code</td>
    <td><a href=""<URL>""><TYPENAME></a></td>
  </tr>
</table>
<!-- end generated table -->

## Description

{descriptor.Description.ToString(CultureInfo.InvariantCulture)}

## Motivation

ADD MOTIVATION HERE

## How to fix violations

ADD HOW TO FIX VIOLATIONS HERE

<!-- start generated config severity -->
## Configure severity

### Via ruleset file.

Configure the severity per project, for more info see [MSDN](https://msdn.microsoft.com/en-us/library/dd264949.aspx).

### Via #pragma directive.
```C#
#pragma warning disable {descriptor.Id} // {descriptor.Title.ToString(CultureInfo.InvariantCulture)}
Code violating the rule here
#pragma warning restore {descriptor.Id} // {descriptor.Title.ToString(CultureInfo.InvariantCulture)}
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable {descriptor.Id} // {descriptor.Title.ToString(CultureInfo.InvariantCulture)}
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage(""{descriptor.Category}"", 
    ""{descriptor.Id}:{descriptor.Title.ToString(CultureInfo.InvariantCulture)}"", 
    Justification = ""Reason..."")]
```
<!-- end generated config severity -->";
            if (Analyzers.Count(x => x.SupportedDiagnostics.Any(d => d.Id == descriptor.Id)) == 1)
            {
                return stub.AssertReplace("<TYPENAME>", descriptorInfo.Analyzer.GetType().Name)
                           .AssertReplace("<URL>", descriptorInfo.AnalyzerFile.Uri);
            }

            var builder = StringBuilderPool.Borrow();
            foreach (var analyzer in Analyzers.Where(x => x.SupportedDiagnostics.Any(d => d.Id == descriptor.Id)))
            {
                _ = builder.AppendLine("  <tr>")
                           .AppendLine($"    <td>{(builder.Length <= "  <tr>\r\n".Length ? "Code" : string.Empty)}</td>")
                           .AppendLine($"    <td><a href=\"{CodeFile.Find(analyzer.GetType()).Uri}\">{analyzer.GetType().Name}</a></td>")
                           .AppendLine("  </tr>");
            }

            var text = builder.Return();
            return stub.Replace("  <tr>\r\n    <td>Code</td>\r\n    <td><a href=\"<URL>\"><TYPENAME></a></td>\r\n  </tr>\r\n", text)
                       .Replace("  <tr>\n    <td>Code</td>\n    <td><a href=\"<URL>\"><TYPENAME></a></td>\n  </tr>\n", text);
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
                this.DocumentationFile = new MarkdownFile(Path.Combine(DocumentsDirectory.FullName, descriptor.Id + ".md"));
                this.AnalyzerFile = CodeFile.Find(analyzer.GetType());
            }

            public DiagnosticAnalyzer Analyzer { get; }

            public DiagnosticDescriptor Descriptor { get; }

            public MarkdownFile DocumentationFile { get; }

            public CodeFile AnalyzerFile { get; }

            public static IEnumerable<DescriptorInfo> Create(DiagnosticAnalyzer analyzer)
            {
                foreach (var descriptor in analyzer.SupportedDiagnostics)
                {
                    yield return new DescriptorInfo(analyzer, descriptor);
                }
            }

            public override string ToString() => this.Descriptor.Id;
        }

        public class MarkdownFile
        {
            public MarkdownFile(string name)
            {
                this.Name = name;
                if (File.Exists(name))
                {
                    this.AllText = File.ReadAllText(name);
                    this.AllLines = File.ReadAllLines(name);
                }
            }

            public string Name { get; }

            public bool Exists => File.Exists(this.Name);

            public string AllText { get; }

            public IReadOnlyList<string> AllLines { get; }
        }

        public class CodeFile
        {
            private static readonly ConcurrentDictionary<Type, CodeFile> Cache = new ConcurrentDictionary<Type, CodeFile>();

            public CodeFile(string name)
            {
                this.Name = name;
            }

            public string Name { get; }

            public string Uri => "https://github.com/DotNetAnalyzers/WpfAnalyzers/blob/master" + this.Name.Substring(SolutionDirectory.FullName.Length)
                                                                                                             .Replace("\\", "/");

            public static CodeFile Find(Type type)
            {
                return Cache.GetOrAdd(type, x => FindCore(x.Name + ".cs"));
            }

            private static CodeFile FindCore(string name)
            {
                var fileName = Cache.Values.Select(x => Path.GetDirectoryName(x.Name))
                                    .Distinct()
                                    .SelectMany(d => Directory.EnumerateFiles(d, name, SearchOption.TopDirectoryOnly))
                                    .FirstOrDefault() ??
                               Directory.EnumerateFiles(SolutionDirectory.FullName, name, SearchOption.AllDirectories)
                                        .First();
                return new CodeFile(fileName);
            }
        }
    }
}
