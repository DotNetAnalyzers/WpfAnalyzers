namespace WpfAnalyzers.Test;

using System.Threading;
using Gu.Roslyn.AnalyzerExtensions;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

public static class BackingFieldOrPropertyTests
{
    [TestCase("nameof(Bar)")]
    [TestCase("\"Bar\"")]
    public static void DependencyPropertyBackingField(string argument)
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            nameof(Bar),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int)this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
        }
    }
}".AssertReplace("nameof(Bar)", argument);
        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, Settings.Default.MetadataReferences);
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var declaration = syntaxTree.FindFieldDeclaration("BarProperty");
        var symbol = semanticModel.GetDeclaredSymbolSafe(declaration, CancellationToken.None);
        var result = BackingFieldOrProperty.Match(symbol)?.RegisteredName(semanticModel, CancellationToken.None);
        Assert.AreEqual(argument, result?.Argument?.ToString());
        Assert.AreEqual("Bar",    result?.Value);
    }

    [TestCase("nameof(Bar)")]
    [TestCase("\"Bar\"")]
    public static void DependencyPropertyBackingProperty(string argument)
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static DependencyProperty BarProperty { get; } = DependencyProperty.Register(
            nameof(Bar),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int)this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
        }
    }
}".AssertReplace("nameof(Bar)", argument);
        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, Settings.Default.MetadataReferences);
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var declaration = syntaxTree.FindPropertyDeclaration("BarProperty");
        var symbol = semanticModel.GetDeclaredSymbol(declaration, CancellationToken.None);
        var result = BackingFieldOrProperty.Match(symbol)?.RegisteredName(semanticModel, CancellationToken.None);
        Assert.AreEqual(argument, result?.Argument?.ToString());
        Assert.AreEqual("Bar",    result?.Value);
    }

    [Test]
    public static void TextElementFontSizePropertyAddOwner()
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Documents;

    public class FooControl : FrameworkElement
    {
        public static readonly DependencyProperty FontSizeProperty = TextElement.FontSizeProperty.AddOwner(typeof(FooControl));

        public double FontSize
        {
            get => (double)this.GetValue(FontSizeProperty);
            set => this.SetValue(FontSizeProperty, value);
        }
    }
}";
        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, Settings.Default.MetadataReferences);
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var declaration = syntaxTree.FindFieldDeclaration("FontSizeProperty");
        var symbol = semanticModel.GetDeclaredSymbolSafe(declaration, CancellationToken.None);
        var result = BackingFieldOrProperty.Match(symbol)?.RegisteredName(semanticModel, CancellationToken.None);
        Assert.AreEqual(null,       result?.Argument);
        Assert.AreEqual("FontSize", result?.Value);
    }

    [Test]
    public static void BorderBorderThicknessPropertyAddOwner()
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : FrameworkElement
    {
        public static readonly DependencyProperty BorderThicknessProperty = Border.BorderThicknessProperty.AddOwner(typeof(FooControl));

        public Size BorderThickness
        {
            get => (Size)GetValue(BorderThicknessProperty);
            set => SetValue(BorderThicknessProperty, value);
        }
    }
}";
        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, Settings.Default.MetadataReferences);
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var declaration = syntaxTree.FindFieldDeclaration("BorderThicknessProperty");
        var symbol = semanticModel.GetDeclaredSymbolSafe(declaration, CancellationToken.None);
        var result = BackingFieldOrProperty.Match(symbol)?.RegisteredName(semanticModel, CancellationToken.None);
        Assert.AreEqual(null,              result?.Argument);
        Assert.AreEqual("BorderThickness", result?.Value);
    }
}
