﻿// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

// ReSharper disable PossibleMultipleEnumeration
namespace WpfAnalyzers.Test
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;
    using NUnit.Framework;

    /// <summary>
    /// Superclass of all unit tests for <see cref="DiagnosticAnalyzer"/>s.
    /// </summary>
    public abstract partial class DiagnosticVerifier
    {
        private const int DefaultIndentationSize = 4;
        private const int DefaultTabSize = 4;
        private const bool DefaultUseTabs = false;

        public DiagnosticVerifier()
        {
            this.IndentationSize = DefaultIndentationSize;
            this.TabSize = DefaultTabSize;
            this.UseTabs = DefaultUseTabs;
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="Microsoft.CodeAnalysis.Formatting.FormattingOptions.IndentationSize"/> to apply to the test
        /// workspace.
        /// </summary>
        /// <value>
        /// The value of the <see cref="Microsoft.CodeAnalysis.Formatting.FormattingOptions.IndentationSize"/> to apply to the test workspace.
        /// </value>
        public int IndentationSize
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="Microsoft.CodeAnalysis.Formatting.FormattingOptions.UseTabs"/> option is applied to the
        /// test workspace.
        /// </summary>
        /// <value>
        /// The value of the <see cref="Microsoft.CodeAnalysis.Formatting.FormattingOptions.UseTabs"/> to apply to the test workspace.
        /// </value>
        public bool UseTabs
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="Microsoft.CodeAnalysis.Formatting.FormattingOptions.TabSize"/> to apply to the test workspace.
        /// </summary>
        /// <value>
        /// The value of the <see cref="Microsoft.CodeAnalysis.Formatting.FormattingOptions.TabSize"/> to apply to the test workspace.
        /// </value>
        public int TabSize
        {
            get;
            protected set;
        }

        protected internal static DiagnosticResult[] EmptyDiagnosticResults { get; } = { };

        public static FileLinePositionSpan GetErrorPosition(string[] testCode)
        {
            var fileNames = CreateFileNamesFromSources(testCode, "cs");
            var line = 0;
            var column = -1;
            var fileName = string.Empty;

            const char errorPositionIndicator = '↓';
            for (var i = 0; i < testCode.Length; i++)
            {
                var source = testCode[i];
                var lineCount = 0;
                foreach (var codeLine in source.Lines())
                {
                    lineCount++;
                    var col = codeLine.IndexOf(errorPositionIndicator);
                    if (col >= 0)
                    {
                        Assert.AreEqual(-1, column, "Expected to find only one error indicator");
                        testCode[i] = testCode[i].Replace(new string(errorPositionIndicator, 1), string.Empty);
                        column = col + 1;
                        line = lineCount;
                        fileName = fileNames[i];
                    }
                }
            }

            Assert.AreNotEqual(-1, column, $"Expected to find one error indicated by {errorPositionIndicator}");
            var pos = new LinePosition(line, column);
            return new FileLinePositionSpan(fileName, pos, pos);
        }

        public static FileLinePositionSpan GetErrorPosition(ref string testCode)
        {
            var sources = new[] { testCode };
            var result = GetErrorPosition(sources);
            testCode = sources[0];
            return result;
        }

        /// <summary>
        /// Verifies that the analyzer will properly handle an empty source.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Test]
        public async Task TestEmptySourceAsync()
        {
            var testCode = string.Empty;
            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public virtual void IdMatches()
        {
            var diagnosticAnalyzer = this.GetCSharpDiagnosticAnalyzers()
                                         .First();
            StringAssert.StartsWith(diagnosticAnalyzer.SupportedDiagnostics.First().Id, diagnosticAnalyzer.GetType().Name);
            StringAssert.Contains(diagnosticAnalyzer.GetType().Name, this.GetType().FullName, "Name of test class does not match analyzer name.");
        }

        /// <summary>
        /// Verifies that each diagnostics contains a <see cref="DiagnosticDescriptor.HelpLinkUri"/> in the expected
        /// format.
        /// </summary>
        [Test]
        public void TestHelpLink()
        {
            var diagnosticAnalyzer = this.GetCSharpDiagnosticAnalyzers()
                                         .First();
            foreach (var diagnostic in diagnosticAnalyzer.SupportedDiagnostics)
            {
                if (diagnostic.DefaultSeverity == DiagnosticSeverity.Hidden && diagnostic.CustomTags.Contains(WellKnownDiagnosticTags.NotConfigurable))
                {
                    Assert.Inconclusive(diagnostic.HelpLinkUri);

                    // This diagnostic will never appear in the UI.
                    continue;
                }

                string expected = $"https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/{diagnostic.Id}.md";
                Assert.AreEqual(expected, diagnostic.HelpLinkUri);
            }
        }

        /// <summary>
        /// Called to test a C# <see cref="DiagnosticAnalyzer"/> when applied on the single input source as a string.
        /// <note type="note">
        /// <para>Input a <see cref="DiagnosticResult"/> for the expected <see cref="Diagnostic"/>.</para>
        /// </note>
        /// </summary>
        /// <param name="source">A class in the form of a string to run the analyzer on.</param>
        /// <param name="expected">A <see cref="DiagnosticResult"/>s describing the <see cref="Diagnostic"/> that should
        /// be reported by the analyzer for the specified source.</param>
        /// <param name="filename">The filename or null if the default filename should be used</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task VerifyCSharpDiagnosticAsync(string source, DiagnosticResult expected, string filename = null)
        {
            return this.VerifyCSharpDiagnosticAsync(source, new[] { expected }, CancellationToken.None, filename);
        }

        /// <summary>
        /// Called to test a C# <see cref="DiagnosticAnalyzer"/> when applied on the single input source as a string.
        /// <note type="note">
        /// <para>Input a <see cref="DiagnosticResult"/> for the expected <see cref="Diagnostic"/>.</para>
        /// </note>
        /// </summary>
        /// <param name="source">A class in the form of a string to run the analyzer on.</param>
        /// <param name="expected">A <see cref="DiagnosticResult"/>s describing the <see cref="Diagnostic"/> that should
        /// be reported by the analyzer for the specified source.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="filename">The filename or null if the default filename should be used</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task VerifyCSharpDiagnosticAsync(string source, DiagnosticResult expected, CancellationToken cancellationToken, string filename = null)
        {
            return this.VerifyCSharpDiagnosticAsync(source, new[] { expected }, cancellationToken, filename);
        }

        /// <summary>
        /// Called to test a C# <see cref="DiagnosticAnalyzer"/> when applied on the single input source as a string.
        /// <note type="note">
        /// <para>Input a <see cref="DiagnosticResult"/> for each <see cref="Diagnostic"/> expected.</para>
        /// </note>
        /// </summary>
        /// <param name="source">A class in the form of a string to run the analyzer on.</param>
        /// <param name="expected">A collection of <see cref="DiagnosticResult"/>s describing the
        /// <see cref="Diagnostic"/>s that should be reported by the analyzer for the specified source.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="filename">The filename or null if the default filename should be used</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task VerifyCSharpDiagnosticAsync(string source, DiagnosticResult[] expected, CancellationToken cancellationToken, string filename = null)
        {
            return this.VerifyDiagnosticsAsync(new[] { source }, LanguageNames.CSharp, this.GetCSharpDiagnosticAnalyzers().ToImmutableArray(), expected, cancellationToken, filename != null ? new[] { filename } : null);
        }

        /// <summary>
        /// Called to test a C# <see cref="DiagnosticAnalyzer"/> when applied on the input strings as sources.
        /// <note type="note">
        /// <para>Input a <see cref="DiagnosticResult"/> for each <see cref="Diagnostic"/> expected.</para>
        /// </note>
        /// </summary>
        /// <param name="sources">A collection of strings to create source documents from to run the analyzers
        /// on.</param>
        /// <param name="expected">A <see cref="DiagnosticResult"/>s describing the <see cref="Diagnostic"/> that should
        /// be reported by the analyzer for the specified source.</param>
        /// <param name="filenames">The filenames or null if the default filename should be used</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task VerifyCSharpDiagnosticAsync(string[] sources, DiagnosticResult expected, string[] filenames = null)
        {
            return this.VerifyDiagnosticsAsync(sources, LanguageNames.CSharp, this.GetCSharpDiagnosticAnalyzers().ToImmutableArray(), new[] { expected }, CancellationToken.None, filenames);
        }

        /// <summary>
        /// Called to test a C# <see cref="DiagnosticAnalyzer"/> when applied on the input strings as sources.
        /// <note type="note">
        /// <para>Input a <see cref="DiagnosticResult"/> for each <see cref="Diagnostic"/> expected.</para>
        /// </note>
        /// </summary>
        /// <param name="sources">A collection of strings to create source documents from to run the analyzers
        /// on.</param>
        /// <param name="expected">A collection of <see cref="DiagnosticResult"/>s describing the
        /// <see cref="Diagnostic"/>s that should be reported by the analyzer for the specified sources.</param>
        /// <param name="filenames">The filenames or null if the default filename should be used</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task VerifyCSharpDiagnosticAsync(string[] sources, DiagnosticResult[] expected, string[] filenames = null)
        {
            return this.VerifyDiagnosticsAsync(sources, LanguageNames.CSharp, this.GetCSharpDiagnosticAnalyzers().ToImmutableArray(), expected, CancellationToken.None, filenames);
        }

        /// <summary>
        /// Called to test a C# <see cref="DiagnosticAnalyzer"/> when applied on the input strings as sources.
        /// <note type="note">
        /// <para>Input a <see cref="DiagnosticResult"/> for each <see cref="Diagnostic"/> expected.</para>
        /// </note>
        /// </summary>
        /// <param name="sources">A collection of strings to create source documents from to run the analyzers
        /// on.</param>
        /// <param name="expected">A <see cref="DiagnosticResult"/>s describing the <see cref="Diagnostic"/> that should
        /// be reported by the analyzer for the specified source.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="filenames">The filenames or null if the default filename should be used</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task VerifyCSharpDiagnosticAsync(string[] sources, DiagnosticResult expected, CancellationToken cancellationToken, string[] filenames = null)
        {
            return this.VerifyDiagnosticsAsync(sources, LanguageNames.CSharp, this.GetCSharpDiagnosticAnalyzers().ToImmutableArray(), new[] { expected }, cancellationToken, filenames);
        }

        /// <summary>
        /// Called to test a C# <see cref="DiagnosticAnalyzer"/> when applied on the input strings as sources.
        /// <note type="note">
        /// <para>Input a <see cref="DiagnosticResult"/> for each <see cref="Diagnostic"/> expected.</para>
        /// </note>
        /// </summary>
        /// <param name="sources">A collection of strings to create source documents from to run the analyzers
        /// on.</param>
        /// <param name="expected">A collection of <see cref="DiagnosticResult"/>s describing the
        /// <see cref="Diagnostic"/>s that should be reported by the analyzer for the specified sources.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="filenames">The filenames or null if the default filename should be used</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task VerifyCSharpDiagnosticAsync(string[] sources, DiagnosticResult[] expected, CancellationToken cancellationToken, string[] filenames = null)
        {
            return this.VerifyDiagnosticsAsync(sources, LanguageNames.CSharp, this.GetCSharpDiagnosticAnalyzers().ToImmutableArray(), expected, cancellationToken, filenames);
        }

        /// <summary>
        /// Gets the C# analyzers being tested
        /// </summary>
        /// <returns>
        /// New instances of all the C# analyzers being tested.
        /// </returns>
        internal abstract IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers();

        [Conditional("DEBUG")]
        //// ReSharper disable once UnusedMember.Local
        private static void DumpIfDebug(string text)
        {
            Console.Write(text);
            Console.WriteLine();
            Console.WriteLine();
        }

        /// <summary>
        /// Checks each of the actual <see cref="Diagnostic"/>s found and compares them with the corresponding
        /// <see cref="DiagnosticResult"/> in the array of expected results. <see cref="Diagnostic"/>s are considered
        /// equal only if the <see cref="DiagnosticResult.Spans"/>, <see cref="DiagnosticResult.Id"/>,
        /// <see cref="DiagnosticResult.Severity"/>, and <see cref="DiagnosticResult.Message"/> of the
        /// <see cref="DiagnosticResult"/> match the actual <see cref="Diagnostic"/>.
        /// </summary>
        /// <param name="actualResults">The <see cref="Diagnostic"/>s found by the compiler after running the analyzer
        /// on the source code.</param>
        /// <param name="analyzers">The analyzers that have been run on the sources.</param>
        /// <param name="expectedResults">A collection of <see cref="DiagnosticResult"/>s describing the expected
        /// diagnostics for the sources.</param>
        private static void VerifyDiagnosticResults(IEnumerable<Diagnostic> actualResults, ImmutableArray<DiagnosticAnalyzer> analyzers, DiagnosticResult[] expectedResults)
        {
            var expectedCount = expectedResults.Length;
            var actualCount = actualResults.Count();

            if (expectedCount != actualCount)
            {
                var diagnosticsOutput = actualResults.Any() ? FormatDiagnostics(analyzers, actualResults.ToArray()) : "NONE.";
                var message = "Mismatch between number of diagnostics returned\r\n" +
                              $"Expected \"{expectedCount}\"\r\n" +
                              $"Actual   \"{actualCount}\"\r\n" +
                               "\r\n" +
                               "Diagnostics:\r\n" +
                              $"{diagnosticsOutput}\r\n";
                Assert.Fail(message);
            }

            for (var i = 0; i < expectedResults.Length; i++)
            {
                var actual = actualResults.ElementAt(i);
                var expected = expectedResults[i];

                if (!expected.HasLocation)
                {
                    if (actual.Location != Location.None)
                    {
                        var message = "Expected:\n" +
                                      "A project diagnostic with No location\n" +
                                      "Actual:\n" +
                                     $"{FormatDiagnostics(analyzers, actual)}";
                        Assert.Fail(message);
                    }
                }
                else
                {
                    VerifyDiagnosticLocation(analyzers, actual, actual.Location, expected.Spans.First());
                    var additionalLocations = actual.AdditionalLocations.ToArray();

                    if (additionalLocations.Length != expected.Spans.Length - 1)
                    {
                        var message = $"Locations differ:\r\n" +
                                      $"Expected: {expected.Spans.Length - 1}\r\n" +
                                      $"Actual:   {additionalLocations.Length}\r\n" +
                                      $"For Diagnostic:\r\n" +
                                      $"    {FormatDiagnostics(analyzers, actual)}\r\n";
                        Assert.Fail(message);
                    }

                    for (var j = 0; j < additionalLocations.Length; ++j)
                    {
                        VerifyDiagnosticLocation(analyzers, actual, additionalLocations[j], expected.Spans[j + 1]);
                    }
                }

                if (actual.Id != expected.Id)
                {
                    var message = $"Diagnostic id differs:\r\n" +
                                  $"Expected: \"{expected.Id}\"\r\n" +
                                  $"Actual:   \"{actual.Id}\"\r\n" +
                                   "\r\n" +
                                   "Diagnostic:\r\n" +
                                  $"    {FormatDiagnostics(analyzers, actual)}\r\n";
                    Assert.Fail(message);
                }

                if (actual.Severity != expected.Severity)
                {
                    var message = $"Diagnostic severity differs:\r\n" +
                                  $"Expected: \"{expected.Severity}\"\r\n" +
                                  $"Actual:   \"{actual.Severity}\"\r\n" +
                                   "\r\n" +
                                   "Diagnostic:\r\n" +
                                  $"    {FormatDiagnostics(analyzers, actual)}\r\n";
                    Assert.Fail(message);
                }

                if (actual.GetMessage() != expected.Message)
                {
                    var message = $"Diagnostic message differs:\r\n" +
                                  $"Expected: \"{expected.Message}\"\r\n" +
                                  $"Actual:   \"{actual.GetMessage()}\"\r\n" +
                                  $"\r\n" +
                                  $"Diagnostic:\r\n" +
                                  $"    {FormatDiagnostics(analyzers, actual)}\r\n";
                    Assert.Fail(message);
                }
            }
        }

        /// <summary>
        /// Helper method to <see cref="VerifyDiagnosticResults"/> that checks the location of a
        /// <see cref="Diagnostic"/> and compares it with the location described by a
        /// <see cref="FileLinePositionSpan"/>.
        /// </summary>
        /// <param name="analyzers">The analyzer that have been run on the sources.</param>
        /// <param name="diagnostic">The diagnostic that was found in the code.</param>
        /// <param name="actual">The location of the diagnostic found in the code.</param>
        /// <param name="expected">The <see cref="FileLinePositionSpan"/> describing the expected location of the
        /// diagnostic.</param>
        private static void VerifyDiagnosticLocation(ImmutableArray<DiagnosticAnalyzer> analyzers, Diagnostic diagnostic, Location actual, FileLinePositionSpan expected)
        {
            var actualSpan = actual.GetLineSpan();
            if (actualSpan.Path != expected.Path)
            {
                var message = "Diagnostic not found in expected file.\r\n" +
                              $"Expected: \"{expected.Path}\"\r\n" +
                              $"Actual:   \"{actualSpan.Path}\"\r\n" +
                               "\r\n" +
                               "Diagnostic:\r\n" +
                              $"    {FormatDiagnostics(analyzers, diagnostic)}\r\n";
                Assert.Fail(message);
            }

            VerifyLinePosition(analyzers, diagnostic, actualSpan.StartLinePosition, expected.StartLinePosition, "start");
            if (expected.StartLinePosition < expected.EndLinePosition)
            {
                VerifyLinePosition(analyzers, diagnostic, actualSpan.EndLinePosition, expected.EndLinePosition, "end");
            }
        }

        private static void VerifyLinePosition(ImmutableArray<DiagnosticAnalyzer> analyzers, Diagnostic diagnostic, LinePosition actualLinePosition, LinePosition expectedLinePosition, string positionText)
        {
            // Only check the line position if it matters
            if (expectedLinePosition.Line > 0 &&
                actualLinePosition.Line + 1 != expectedLinePosition.Line)
            {
                var formattedDiagnostics = FormatDiagnostics(analyzers, diagnostic);
                var message = $"Diagnostic: {positionText}\r\n" +
                              $"Expected diagnostic on line \"{expectedLinePosition.Line}\"\r\n" +
                              $"Actual   diagnostic on line \"{actualLinePosition.Line + 1}\"\r\n" +
                               "\r\n" +
                               "Diagnostic:\r\n" +
                              $"{formattedDiagnostics}\r\n";
                Assert.Fail(message);
            }

            // Only check the column position if it matters
            if (expectedLinePosition.Character > 0 &&
               actualLinePosition.Character + 1 != expectedLinePosition.Character)
            {
                var formattedDiagnostics = FormatDiagnostics(analyzers, diagnostic);
                var message = $"Diagnostic: {positionText}\r\n" +
                              $"Expected diagnostic at column \"{expectedLinePosition.Character}\"\r\n" +
                              $"Actual   diagnostic at column \"{actualLinePosition.Character + 1}\"\r\n" +
                               "\r\n" +
                               "Diagnostic:\r\n" +
                              $"{formattedDiagnostics}\r\n";
                Assert.Fail(message);
            }
        }

        /// <summary>
        /// Helper method to format a <see cref="Diagnostic"/> into an easily readable string.
        /// </summary>
        /// <param name="analyzers">The analyzers that this verifier tests.</param>
        /// <param name="diagnostics">A collection of <see cref="Diagnostic"/>s to be formatted.</param>
        /// <returns>The <paramref name="diagnostics"/> formatted as a string.</returns>
        private static string FormatDiagnostics(ImmutableArray<DiagnosticAnalyzer> analyzers, params Diagnostic[] diagnostics)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < diagnostics.Length; ++i)
            {
                var diagnosticsId = diagnostics[i].Id;

                builder.Append("// ").AppendLine(diagnostics[i].ToString());

                var applicableAnalyzer = analyzers.FirstOrDefault(a => a.SupportedDiagnostics.Any(dd => dd.Id == diagnosticsId));
                if (applicableAnalyzer != null)
                {
                    var analyzerType = applicableAnalyzer.GetType();

                    var location = diagnostics[i].Location;
                    if (location == Location.None)
                    {
                        builder.AppendFormat("GetGlobalResult({0}.{1})", analyzerType.Name, diagnosticsId);
                    }
                    else
                    {
                        Assert.True(
                            location.IsInSource,
                            $"Test base does not currently handle diagnostics in metadata locations. Diagnostic in metadata:\r\n{diagnostics[i]}");

                        var resultMethodName = diagnostics[i].Location.SourceTree.FilePath.EndsWith(".cs") ? "GetCSharpResultAt" : "GetBasicResultAt";
                        var linePosition = diagnostics[i].Location.GetLineSpan().StartLinePosition;

                        builder.AppendFormat(
                            "{0}@({1}, {2}, {3}.{4})",
                            resultMethodName,
                            linePosition.Line + 1,
                            linePosition.Character + 1,
                            analyzerType.Name,
                            diagnosticsId);
                    }

                    if (i != diagnostics.Length - 1)
                    {
                        builder.Append(',');
                    }

                    builder.AppendLine();
                }
            }

            return builder.ToString();
        }

        private static bool IsSubjectToExclusion(DiagnosticResult result)
        {
            if (result.Id.StartsWith("CS", StringComparison.Ordinal))
            {
                return false;
            }

            if (result.Id.StartsWith("AD", StringComparison.Ordinal))
            {
                return false;
            }

            if (result.Spans.Length == 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// General method that gets a collection of actual <see cref="Diagnostic"/>s found in the source after the
        /// analyzer is run, then verifies each of them.
        /// </summary>
        /// <param name="sources">An array of strings to create source documents from to run the analyzers on.</param>
        /// <param name="language">The language of the classes represented by the source strings.</param>
        /// <param name="analyzers">The analyzers to be run on the source code.</param>
        /// <param name="expected">A collection of <see cref="DiagnosticResult"/>s that should appear after the analyzer
        /// is run on the sources.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <param name="filenames">The filenames or null if the default filename should be used</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task VerifyDiagnosticsAsync(string[] sources, string language, ImmutableArray<DiagnosticAnalyzer> analyzers, DiagnosticResult[] expected, CancellationToken cancellationToken, string[] filenames)
        {
            VerifyDiagnosticResults(await this.GetSortedDiagnosticsAsync(sources, language, analyzers, cancellationToken, filenames).ConfigureAwait(false), analyzers, expected);

            // If filenames is null we want to test for exclusions too
            if (filenames == null)
            {
                // Also check if the analyzer honors exclusions
                if (expected.Any(IsSubjectToExclusion))
                {
                    // Diagnostics reported by the compiler and analyzer diagnostics which don't have a location will
                    // still be reported. We also insert a new line at the beginning so we have to move all diagnostic
                    // locations which have a specific position down by one line.
                    var expectedResults = expected
                        .Where(x => !IsSubjectToExclusion(x))
                        .Select(x => x.WithLineOffset(1))
                        .ToArray();

                    VerifyDiagnosticResults(await this.GetSortedDiagnosticsAsync(sources.Select(x => " // <auto-generated>\r\n" + x).ToArray(), language, analyzers, cancellationToken, null).ConfigureAwait(false), analyzers, expectedResults);
                }
            }
        }
    }
}
