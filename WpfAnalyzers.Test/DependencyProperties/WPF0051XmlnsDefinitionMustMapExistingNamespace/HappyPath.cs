namespace WpfAnalyzers.Test.DependencyProperties.WPF0051XmlnsDefinitionMustMapExistingNamespace
{
    using System.Threading.Tasks;

    using NUnit.Framework;

    using WpfAnalyzers.DependencyProperties;

    internal class HappyPath : HappyPathVerifier<WPF0051XmlnsDefinitionMustMapExistingNamespace>
    {
        [Test]
        public async Task WhenXmlnsDefinitionMatches()
        {
            var controlCode = @"namespace Gu.Wpf.Geometry
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
[assembly: XmlnsDefinition(""http://gu.se/Geometry"", ""Gu.Wpf.Geometry"")]";
            await this.VerifyHappyPathAsync(new[] { testCode, controlCode }, new[] { "AssemblyInfo.cs", "FooControl.cs" }).ConfigureAwait(false);
        }

        [Test]
        public async Task WhenTwoXmlnsDefinitions()
        {

            var controlCode1 = @"namespace Gu.Wpf.Geometry
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public class FooControl1 : Control
    {
        public static readonly DependencyProperty BrushProperty = DependencyProperty.Register(
            nameof(Brush),
            typeof(Brush),
            typeof(FooControl1),
            new PropertyMetadata(default(Brush)));

        public Brush Brush
        {
            get { return (Brush)this.GetValue(BrushProperty); }
            set { this.SetValue(BrushProperty, value); }
        }
    }
}";


            var controlCode2 = @"namespace Gu.Wpf.Geometry.Balloons
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public class FooControl2 : Control
    {
        public static readonly DependencyProperty BrushProperty = DependencyProperty.Register(
            nameof(Brush),
            typeof(Brush),
            typeof(FooControl2),
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
[assembly: XmlnsDefinition(""http://gu.se/Geometry"", ""Gu.Wpf.Geometry"")]
[assembly: XmlnsDefinition(""http://gu.se/Geometry"", ""Gu.Wpf.Geometry.Balloons"")]";

            await this.VerifyHappyPathAsync(new[] { testCode, controlCode1, controlCode2 }, new[] { "AssemblyInfo.cs", "FooControl1.cs", "FooControl2.cs" }).ConfigureAwait(false);
        }
    }
}
