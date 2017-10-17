namespace WpfAnalyzers.Test.WPF0052XmlnsDefinitionsDoesNotMapAllNamespacesTests
{
    using System.Threading.Tasks;
    using NUnit.Framework;

    internal class CodeFix : CodeFixVerifier<WPF0052XmlnsDefinitionsDoesNotMapAllNamespaces, AddXmlnsDefinitionCodeFixProvider>
    {
        [Test]
        public async Task WhenMissingNamespace()
        {
            var control1Code = @"namespace Gu.Wpf.Geometry
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BrushProperty = DependencyProperty.Register(
            nameof(Brush),
            typeof(Brush),
            typeof(FooControl),
            new PropertyMetadata(default(Brush)));

        public Brush Brush
        {
            get { return (Brush)this.GetValue(BrushProperty); }
            set { this.SetValue(BrushProperty, value); }
        }
    }
}";

            var control2Code = @"namespace Gu.Wpf.Geometry.Meh
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public class BarControl : Control
    {
        public static readonly DependencyProperty BrushProperty = DependencyProperty.Register(
            nameof(Brush),
            typeof(Brush),
            typeof(BarControl),
            new PropertyMetadata(default(Brush)));

        public Brush Brush
        {
            get { return (Brush)this.GetValue(BrushProperty); }
            set { this.SetValue(BrushProperty, value); }
        }
    }
}";

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
                               .WithMessage("XmlnsDefinitions does not map all namespaces with public types.");

            await this.VerifyCSharpDiagnosticAsync(new[] { control1Code, control2Code, testCode }, expected).ConfigureAwait(false);

            var fixedCode = @"
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
[assembly: XmlnsDefinition(""http://gu.se/Geometry"", ""Gu.Wpf.Geometry"")]
[assembly: XmlnsDefinition(""http://gu.se/Geometry"", ""Gu.Wpf.Geometry.Meh"")]";

            await this.VerifyCSharpFixAsync(new[] { control1Code, control2Code, testCode }, new[] { control1Code, control2Code, fixedCode }).ConfigureAwait(false);
        }

        [Test]
        public async Task WhenMissingNamespaceWithNameColon()
        {
            var control1Code = @"namespace Gu.Wpf.Geometry
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BrushProperty = DependencyProperty.Register(
            nameof(Brush),
            typeof(Brush),
            typeof(FooControl),
            new PropertyMetadata(default(Brush)));

        public Brush Brush
        {
            get { return (Brush)this.GetValue(BrushProperty); }
            set { this.SetValue(BrushProperty, value); }
        }
    }
}";

            var control2Code = @"namespace Gu.Wpf.Geometry.Meh
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public class BarControl : Control
    {
        public static readonly DependencyProperty BrushProperty = DependencyProperty.Register(
            nameof(Brush),
            typeof(Brush),
            typeof(BarControl),
            new PropertyMetadata(default(Brush)));

        public Brush Brush
        {
            get { return (Brush)this.GetValue(BrushProperty); }
            set { this.SetValue(BrushProperty, value); }
        }
    }
}";

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
[assembly: ↓XmlnsDefinition(""http://gu.se/Geometry"", clrNamespace: ""Gu.Wpf.Geometry"")]";

            var expected = this.CSharpDiagnostic()
                               .WithLocationIndicated(ref testCode)
                               .WithMessage("XmlnsDefinitions does not map all namespaces with public types.");

            await this.VerifyCSharpDiagnosticAsync(new[] { control1Code, control2Code, testCode }, expected).ConfigureAwait(false);

            var fixedCode = @"
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
[assembly: XmlnsDefinition(""http://gu.se/Geometry"", clrNamespace: ""Gu.Wpf.Geometry"")]
[assembly: XmlnsDefinition(""http://gu.se/Geometry"", clrNamespace: ""Gu.Wpf.Geometry.Meh"")]";

            await this.VerifyCSharpFixAsync(new[] { control1Code, control2Code, testCode }, new[] { control1Code, control2Code, fixedCode }).ConfigureAwait(false);
        }
    }
}