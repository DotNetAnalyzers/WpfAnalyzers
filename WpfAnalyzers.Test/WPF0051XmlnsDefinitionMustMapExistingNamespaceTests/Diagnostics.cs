namespace WpfAnalyzers.Test.WPF0051XmlnsDefinitionMustMapExistingNamespaceTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class Diagnostics
    {
        [Test]
        public void WhenNoNamespace()
        {
            var testCode = @"
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Markup;

[assembly: AssemblyTitle(""Gu.Wpf.Geometry"")]
[assembly: AssemblyDescription(""Geometries for WPF."")]
[assembly: AssemblyConfiguration("""")]
[assembly: AssemblyCompany(""Johan Larsson"")]
[assembly: AssemblyProduct(""Gu.Wpf.Geometry"")]
[assembly: AssemblyCopyright(""Copyright ©  2015"")]
[assembly: AssemblyTrademark("""")]
[assembly: AssemblyCulture("""")]
[assembly: ComVisible(false)]
[assembly: Guid(""a9cbadf5-65f8-4a81-858c-ea0cdfeb2e99"")]
[assembly: AssemblyVersion(""1.0.0.2"")]
[assembly: AssemblyFileVersion(""1.0.0.2"")]
[assembly: InternalsVisibleTo(""Gu.Wpf.Geometry.Tests"", AllInternalsVisible = true)]
[assembly: InternalsVisibleTo(""Gu.Wpf.Geometry.Demo"", AllInternalsVisible = true)]
[assembly: InternalsVisibleTo(""Gu.Wpf.Geometry.Benchmarks"", AllInternalsVisible = true)]

[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]
[assembly: XmlnsDefinition(""http://gu.se/Geometry"", ↓""Gu.Wpf.Geometry"")]";
            var expectedDiagnostic = ExpectedDiagnostic.CreateFromCodeWithErrorsIndicated(
                "WPF0051",
                "[XmlnsDefinition] maps to \'\"Gu.Wpf.Geometry\"\' that does not exist.",
                testCode,
                out testCode);
            AnalyzerAssert.Diagnostics<WPF0051XmlnsDefinitionMustMapExistingNamespace>(expectedDiagnostic, testCode);
        }

        [Test]
        public void WhenMissingNamespace()
        {
            var testCode = @"
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Markup;

[assembly: AssemblyTitle(""Gu.Wpf.Geometry"")]
[assembly: AssemblyDescription(""Geometries for WPF."")]
[assembly: AssemblyConfiguration("""")]
[assembly: AssemblyCompany(""Johan Larsson"")]
[assembly: AssemblyProduct(""Gu.Wpf.Geometry"")]
[assembly: AssemblyCopyright(""Copyright ©  2015"")]
[assembly: AssemblyTrademark("""")]
[assembly: AssemblyCulture("""")]
[assembly: ComVisible(false)]
[assembly: Guid(""a9cbadf5-65f8-4a81-858c-ea0cdfeb2e99"")]
[assembly: AssemblyVersion(""1.0.0.2"")]
[assembly: AssemblyFileVersion(""1.0.0.2"")]
[assembly: InternalsVisibleTo(""Gu.Wpf.Geometry.Tests"", AllInternalsVisible = true)]
[assembly: InternalsVisibleTo(""Gu.Wpf.Geometry.Demo"", AllInternalsVisible = true)]
[assembly: InternalsVisibleTo(""Gu.Wpf.Geometry.Benchmarks"", AllInternalsVisible = true)]

[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]
[assembly: XmlnsDefinition(""http://gu.se/Geometry"", ↓""Gu.Wpf.Geometry"")]";
            var expectedDiagnostic = ExpectedDiagnostic.CreateFromCodeWithErrorsIndicated(
                "WPF0051",
                "[XmlnsDefinition] maps to \'\"Gu.Wpf.Geometry\"\' that does not exist.",
                testCode,
                out testCode);
            AnalyzerAssert.Diagnostics<WPF0051XmlnsDefinitionMustMapExistingNamespace>(expectedDiagnostic, testCode);
        }
    }
}