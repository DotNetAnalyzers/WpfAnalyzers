namespace WpfAnalyzers.Test
{
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    public static class DependencyObjectTests
    {
        [TestCase("SetValue(BarProperty, value)")]
        [TestCase("this.SetValue(BarProperty, value)")]
        [TestCase("base.SetValue(BarProperty, value)")]
        public static void SetValueMatch(string call)
        {
            var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            ""Bar"",
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int)this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
        }
    }
}".AssertReplace("this.SetValue(BarProperty, value)", call);

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, Settings.Default.MetadataReferences);
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var invocation = syntaxTree.FindInvocation("SetValue");
            Assert.AreEqual("SetValue", DependencyObject.SetValue.Match(invocation, semanticModel, CancellationToken.None)?.Target.Name);

            invocation = syntaxTree.FindInvocation("GetValue");
            Assert.AreEqual(null, DependencyObject.SetValue.Match(invocation, semanticModel, CancellationToken.None));
        }

        [TestCase(".SetValue")]
        [TestCase("?.SetValue")]
        public static void SetValueMatchInstance(string call)
        {
            var code = @"
namespace N
{
    using System.Windows;

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

        public static int GetBar(DependencyObject element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}".AssertReplace(".SetValue", call);
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, Settings.Default.MetadataReferences);
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var invocation = syntaxTree.FindInvocation("SetValue");
            Assert.AreEqual("SetValue", DependencyObject.SetValue.Match(invocation, semanticModel, CancellationToken.None)?.Target.Name);

            invocation = syntaxTree.FindInvocation("GetValue");
            Assert.AreEqual(null, DependencyObject.SetValue.Match(invocation, semanticModel, CancellationToken.None));
        }

        [TestCase("SetCurrentValue(BarProperty, value)")]
        [TestCase("this.SetCurrentValue(BarProperty, value)")]
        [TestCase("base.SetCurrentValue(BarProperty, value)")]
        public static void SetCurrentValueMatch(string call)
        {
            var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            ""Bar"",
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int)this.GetValue(BarProperty); }
            set { this.SetCurrentValue(BarProperty, value); }
        }
    }
}".AssertReplace("this.SetCurrentValue(BarProperty, value)", call);

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, Settings.Default.MetadataReferences);
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var invocation = syntaxTree.FindInvocation("SetCurrentValue");
            Assert.AreEqual("SetCurrentValue", DependencyObject.SetCurrentValue.Match(invocation, semanticModel, CancellationToken.None)?.Target.Name);

            invocation = syntaxTree.FindInvocation("GetValue");
            Assert.AreEqual(null, DependencyObject.SetCurrentValue.Match(invocation, semanticModel, CancellationToken.None));
        }

        [Test]
        public static void SetCurrentValueMatchInstance()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
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

        public Foo()
        {
            new Control().SetCurrentValue(BarProperty, 1);
        }
    }
}");
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, Settings.Default.MetadataReferences);
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var invocation = syntaxTree.FindInvocation("SetCurrentValue");
            Assert.AreEqual("SetCurrentValue", DependencyObject.SetCurrentValue.Match(invocation, semanticModel, CancellationToken.None)?.Target.Name);

            invocation = syntaxTree.FindInvocation("RegisterAttached");
            Assert.AreEqual(null, DependencyObject.SetCurrentValue.Match(invocation, semanticModel, CancellationToken.None));
        }

        [TestCase("GetValue(BarProperty)")]
        [TestCase("this.GetValue(BarProperty)")]
        [TestCase("base.GetValue(BarProperty)")]
        public static void GetValueMatch(string call)
        {
            var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            ""Bar"",
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int)this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
        }
    }
}".AssertReplace("this.GetValue(BarProperty)", call);

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, Settings.Default.MetadataReferences);
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var invocation = syntaxTree.FindInvocation("GetValue");
            Assert.AreEqual("GetValue", DependencyObject.GetValue.Match(invocation, semanticModel, CancellationToken.None)?.Target.Name);

            invocation = syntaxTree.FindInvocation("SetValue");
            Assert.AreEqual(null, DependencyObject.GetValue.Match(invocation, semanticModel, CancellationToken.None));
        }

        [Test]
        public static void GetValueMatchInstance()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace N
{
    using System.Windows;

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

        public static int GetBar(DependencyObject element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}");
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, Settings.Default.MetadataReferences);
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var invocation = syntaxTree.FindInvocation("GetValue");
            Assert.AreEqual("GetValue", DependencyObject.GetValue.Match(invocation, semanticModel, CancellationToken.None)?.Target.Name);

            invocation = syntaxTree.FindInvocation("SetValue");
            Assert.AreEqual(null, DependencyObject.GetValue.Match(invocation, semanticModel, CancellationToken.None));
        }
    }
}
