namespace WpfAnalyzers.Test.DependencyProperties.WPF0051XmlnsDefinitionMustMapExistingNamespace
{
    using System.Threading.Tasks;

    using NUnit.Framework;

    using WpfAnalyzers.DependencyProperties;

    internal class Diagnostics : DiagnosticVerifier<WPF0051XmlnsDefinitionMustMapExistingNamespace>
    {
        [Test]
        public async Task WhenNoNamespace()
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
            var expected = this.CSharpDiagnostic()
                               .WithLocationIndicated(ref testCode)
                               .WithMessage("[XmlnsDefinition] maps to \'\"Gu.Wpf.Geometry\"\' that does not exist.");

            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);
        }

        [Test]
        public async Task WhenMissingNamespace()
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
            var expected = this.CSharpDiagnostic()
                               .WithLocationIndicated(ref testCode)
                               .WithMessage("[XmlnsDefinition] maps to \'\"Gu.Wpf.Geometry\"\' that does not exist.");

            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);
        }
    }
}