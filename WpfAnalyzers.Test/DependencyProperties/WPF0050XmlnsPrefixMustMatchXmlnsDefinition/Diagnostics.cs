namespace WpfAnalyzers.Test.DependencyProperties.WPF0050XmlnsPrefixMustMatchXmlnsDefinition
{
    using System.Threading.Tasks;

    using NUnit.Framework;

    using WpfAnalyzers.DependencyProperties;

    internal class Diagnostics : DiagnosticVerifier<WPF0050XmlnsPrefixMustMatchXmlnsDefinition>
    {
        [Test]
        public async Task WhenNoXmlnsDefinition()
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
[assembly: ↓XmlnsPrefix(""http://gu.se/Geometry"", ""geometry"")]";
            var expected = this.CSharpDiagnostic()
                               .WithLocationIndicated(ref testCode)
                               .WithMessage("There is no [XmlnsDefinition] mapping to 'http://gu.se/Geometry'");

            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);
        }

        [Test]
        public async Task WhenNoXmlnsPrefix()
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
[assembly: ↓XmlnsDefinition(""http://gu.se/Geometry"", ""Gu.Wpf.Geometry"")]";
            var expected = this.CSharpDiagnostic()
                               .WithLocationIndicated(ref testCode)
                               .WithMessage("There is no [XmlnsPrefix] mapping to 'http://gu.se/Geometry'");

            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);
        }

        [Test]
        public async Task WhenXmlnsDefinitionIsNotMatching()
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
[assembly: XmlnsDefinition(""http://gu.se/SomewhereElse"", ""Gu.Wpf.Geometry"")]
[assembly: XmlnsPrefix(""http://gu.se/Geometry"", ""geometry"")]";

            var assemblyinfo = "AssemblyInfo.cs";

            var expected1 = this.CSharpDiagnostic()
                               .WithLocation(assemblyinfo, 25, 12)
                               .WithMessage("There is no [XmlnsPrefix] mapping to 'http://gu.se/SomewhereElse'");
            var expected2 = this.CSharpDiagnostic()
                                .WithLocation(assemblyinfo, 26, 12)
                                .WithMessage("There is no [XmlnsDefinition] mapping to 'http://gu.se/Geometry'");
            await this.VerifyCSharpDiagnosticAsync(new[] { testCode }, new[] { expected1, expected2 }, new[] { assemblyinfo }).ConfigureAwait(false);
        }
    }
}