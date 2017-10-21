namespace WpfAnalyzers.Test
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using NUnit.Framework;

    public class DependencyObjectTests
    {
        [TestCase("SetValue(BarProperty, value)")]
        [TestCase("this.SetValue(BarProperty, value)")]
        [TestCase("base.SetValue(BarProperty, value)")]
        public void IsPotentialSetValueCall(string setCall)
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
}";
            testCode = testCode.AssertReplace("this.SetValue(BarProperty, value)", setCall);

            var syntaxTree = CSharpSyntaxTree.ParseText(testCode);
            var invocation = syntaxTree.FindBestMatch<InvocationExpressionSyntax>("SetValue");
            Assert.AreEqual(true, DependencyObject.IsPotentialSetValueCall(invocation));

            invocation = syntaxTree.FindBestMatch<InvocationExpressionSyntax>("RegisterAttached");
            Assert.AreEqual(false, DependencyObject.IsPotentialSetValueCall(invocation));
        }

        [Test]
        public void IsPotentialSetValueCallInstance()
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
            var invocation = syntaxTree.FindBestMatch<InvocationExpressionSyntax>("SetValue");
            Assert.AreEqual(true, DependencyObject.IsPotentialSetValueCall(invocation));

            invocation = syntaxTree.FindBestMatch<InvocationExpressionSyntax>("RegisterAttached");
            Assert.AreEqual(false, DependencyObject.IsPotentialSetValueCall(invocation));
        }

        [TestCase("SetCurrentValue(BarProperty, value)")]
        [TestCase("this.SetCurrentValue(BarProperty, value)")]
        [TestCase("base.SetCurrentValue(BarProperty, value)")]
        public void IsPotentialSetCurrentValue(string setCall)
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
}";
            testCode = testCode.AssertReplace("this.SetCurrentValue(BarProperty, value)", setCall);

            var syntaxTree = CSharpSyntaxTree.ParseText(testCode);
            var invocation = syntaxTree.FindBestMatch<InvocationExpressionSyntax>("SetCurrentValue");
            Assert.AreEqual(true, DependencyObject.IsPotentialSetValueCall(invocation));

            invocation = syntaxTree.FindBestMatch<InvocationExpressionSyntax>("RegisterAttached");
            Assert.AreEqual(false, DependencyObject.IsPotentialSetValueCall(invocation));
        }

        [Test]
        public void IsPotentialSetCurrentValueCallInstance()
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
            var invocation = syntaxTree.FindBestMatch<InvocationExpressionSyntax>("SetCurrentValue");
            Assert.AreEqual(true, DependencyObject.IsPotentialSetCurrentValueCall(invocation));

            invocation = syntaxTree.FindBestMatch<InvocationExpressionSyntax>("RegisterAttached");
            Assert.AreEqual(false, DependencyObject.IsPotentialSetCurrentValueCall(invocation));
        }

        [TestCase("GetValue(BarProperty)")]
        [TestCase("this.GetValue(BarProperty)")]
        [TestCase("base.GetValue(BarProperty)")]
        public void IsPotentialGetValueCall(string setCall)
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
}";
            testCode = testCode.AssertReplace("this.GetValue(BarProperty)", setCall);

            var syntaxTree = CSharpSyntaxTree.ParseText(testCode);
            var invocation = syntaxTree.FindBestMatch<InvocationExpressionSyntax>("GetValue");
            Assert.AreEqual(true, DependencyObject.IsPotentialSetValueCall(invocation));

            invocation = syntaxTree.FindBestMatch<InvocationExpressionSyntax>("RegisterAttached");
            Assert.AreEqual(false, DependencyObject.IsPotentialSetValueCall(invocation));
        }

        [Test]
        public void IsPotentialGetValueCallInstance()
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
            var invocation = syntaxTree.FindBestMatch<InvocationExpressionSyntax>("GetValue");
            Assert.AreEqual(true, DependencyObject.IsPotentialGetValueCall(invocation));

            invocation = syntaxTree.FindBestMatch<InvocationExpressionSyntax>("RegisterAttached");
            Assert.AreEqual(false, DependencyObject.IsPotentialGetValueCall(invocation));
        }
    }
}