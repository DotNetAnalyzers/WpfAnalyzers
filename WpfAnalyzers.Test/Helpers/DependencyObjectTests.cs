namespace WpfAnalyzers.Test
{
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    public class DependencyObjectTests
    {
        [TestCase("SetValue(BarProperty, value)")]
        [TestCase("this.SetValue(BarProperty, value)")]
        [TestCase("base.SetValue(BarProperty, value)")]
        public void TryGetSetValueCall(string call)
        {
            var testCode = @"
namespace RoslynSandbox
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

            var syntaxTree = CSharpSyntaxTree.ParseText(testCode);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, RoslynAssert.MetadataReferences);
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var invocation = syntaxTree.FindInvocation("SetValue");
            Assert.AreEqual(true, DependencyObject.TryGetSetValueCall(invocation, semanticModel, CancellationToken.None, out var method));
            Assert.AreEqual("SetValue", method.Name);

            invocation = syntaxTree.FindInvocation("GetValue");
            Assert.AreEqual(false, DependencyObject.TryGetSetValueCall(invocation, semanticModel, CancellationToken.None, out method));
        }

        [TestCase(".SetValue")]
        [TestCase("?.SetValue")]
        public void TryGetSetValueCallInstance(string call)
        {
            var testCode = @"
namespace RoslynSandbox
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
            var syntaxTree = CSharpSyntaxTree.ParseText(testCode);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, RoslynAssert.MetadataReferences);
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var invocation = syntaxTree.FindInvocation("SetValue");
            Assert.AreEqual(true, DependencyObject.TryGetSetValueCall(invocation, semanticModel, CancellationToken.None, out var method));
            Assert.AreEqual("SetValue", method.Name);

            invocation = syntaxTree.FindInvocation("GetValue");
            Assert.AreEqual(false, DependencyObject.TryGetSetValueCall(invocation, semanticModel, CancellationToken.None, out method));
        }

        [TestCase("SetCurrentValue(BarProperty, value)")]
        [TestCase("this.SetCurrentValue(BarProperty, value)")]
        [TestCase("base.SetCurrentValue(BarProperty, value)")]
        public void TryGetSetCurrentValueCall(string call)
        {
            var testCode = @"
namespace RoslynSandbox
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

            var syntaxTree = CSharpSyntaxTree.ParseText(testCode);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, RoslynAssert.MetadataReferences);
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var invocation = syntaxTree.FindInvocation("SetCurrentValue");
            Assert.AreEqual(true, DependencyObject.TryGetSetCurrentValueCall(invocation, semanticModel, CancellationToken.None, out var method));
            Assert.AreEqual("SetCurrentValue", method.Name);

            invocation = syntaxTree.FindInvocation("GetValue");
            Assert.AreEqual(false, DependencyObject.TryGetSetCurrentValueCall(invocation, semanticModel, CancellationToken.None, out method));
        }

        [Test]
        public void TryGetSetCurrentValueCallInstance()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandbox
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
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, RoslynAssert.MetadataReferences);
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var invocation = syntaxTree.FindInvocation("SetCurrentValue");
            Assert.AreEqual(true, DependencyObject.TryGetSetCurrentValueCall(invocation, semanticModel, CancellationToken.None, out var method));
            Assert.AreEqual("SetCurrentValue", method.Name);

            invocation = syntaxTree.FindInvocation("RegisterAttached");
            Assert.AreEqual(false, DependencyObject.TryGetSetCurrentValueCall(invocation, semanticModel, CancellationToken.None, out method));
        }

        [TestCase("GetValue(BarProperty)")]
        [TestCase("this.GetValue(BarProperty)")]
        [TestCase("base.GetValue(BarProperty)")]
        public void TryGetGetValueCall(string call)
        {
            var testCode = @"
namespace RoslynSandbox
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

            var syntaxTree = CSharpSyntaxTree.ParseText(testCode);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, RoslynAssert.MetadataReferences);
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var invocation = syntaxTree.FindInvocation("GetValue");
            Assert.AreEqual(true, DependencyObject.TryGetGetValueCall(invocation, semanticModel, CancellationToken.None, out var method));
            Assert.AreEqual("GetValue", method.Name);

            invocation = syntaxTree.FindInvocation("SetValue");
            Assert.AreEqual(false, DependencyObject.TryGetGetValueCall(invocation, semanticModel, CancellationToken.None, out method));
        }

        [Test]
        public void TryGetSetValueCallInstance()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandbox
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
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, RoslynAssert.MetadataReferences);
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var invocation = syntaxTree.FindInvocation("GetValue");
            Assert.AreEqual(true, DependencyObject.TryGetGetValueCall(invocation, semanticModel, CancellationToken.None, out var method));
            Assert.AreEqual("GetValue", method.Name);

            invocation = syntaxTree.FindInvocation("SetValue");
            Assert.AreEqual(false, DependencyObject.TryGetGetValueCall(invocation, semanticModel, CancellationToken.None, out method));
        }
    }
}
