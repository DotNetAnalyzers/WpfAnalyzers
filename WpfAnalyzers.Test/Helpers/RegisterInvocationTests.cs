namespace WpfAnalyzers.Test
{
    using System.Threading;

    using Gu.Roslyn.Asserts;

    using Microsoft.CodeAnalysis.CSharp;

    using NUnit.Framework;

    public static class DependencyPropertyRegisterTests
    {
        [Test]
        public static void AddOwnerMatch()
        {
            var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        public static void SetBar(DependencyObject element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        [AttachedPropertyBrowsableForChildren(IncludeDescendants = false)]
        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static int GetBar(DependencyObject element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = Foo.BarProperty.AddOwner(typeof(FooControl));

        public double Bar
        {
            get { return (double)this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
        }
    }
}";
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var invocation = syntaxTree.FindInvocation("AddOwner");
            Assert.AreEqual("AddOwner", DependencyProperty.AddOwner.Match(invocation, semanticModel, CancellationToken.None)?.Target.Name);

            invocation = syntaxTree.FindInvocation("GetValue");
            Assert.AreEqual(null, DependencyProperty.AddOwner.Match(invocation, semanticModel, CancellationToken.None));
        }

        [Test]
        public static void TryGetOverrideMetadataCall()
        {
            var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public int Value
        {
            get { return (int)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }
    }

    public class BarControl : FooControl
    {
        static BarControl()
        {
            ValueProperty.OverrideMetadata(typeof(BarControl), new PropertyMetadata(1));
        }
    }
}";
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var invocation = syntaxTree.FindInvocation("OverrideMetadata");
            Assert.AreEqual(true, DependencyProperty.TryGetOverrideMetadataCall(invocation, semanticModel, CancellationToken.None, out var method));
            Assert.AreEqual("OverrideMetadata", method.Name);

            invocation = syntaxTree.FindInvocation("GetValue");
            Assert.AreEqual(false, DependencyProperty.TryGetOverrideMetadataCall(invocation, semanticModel, CancellationToken.None, out _));
        }
    }
}
