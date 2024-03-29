﻿namespace WpfAnalyzers.Test;

using System.Threading;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

public static partial class DependencyPropertyTests
{
    public static class Register
    {
        [Test]
        public static void MatchRegister()
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
}";
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, Settings.Default.MetadataReferences);
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var invocation = syntaxTree.FindInvocation("Register");
            Assert.AreEqual("Register", DependencyProperty.Register.MatchRegister(invocation, semanticModel, CancellationToken.None)?.Target.Name);
            Assert.AreEqual("Register", DependencyProperty.Register.MatchAny(invocation, semanticModel, CancellationToken.None)?.Target.Name);

            invocation = syntaxTree.FindInvocation("GetValue");
            Assert.AreEqual(null, DependencyProperty.Register.MatchRegister(invocation, semanticModel, CancellationToken.None));
            Assert.AreEqual(null, DependencyProperty.Register.MatchAny(invocation, semanticModel, CancellationToken.None));
        }

        [Test]
        public static void MatchRegisterReadOnly()
        {
            var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterReadOnly(
            ""Bar"",
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

        public int Bar
        {
            get { return (int)this.GetValue(BarProperty); }
            protected set { this.SetValue(BarPropertyKey, value); }
        }
    }
}";
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, Settings.Default.MetadataReferences);
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var invocation = syntaxTree.FindInvocation("RegisterReadOnly");
            Assert.AreEqual("RegisterReadOnly", DependencyProperty.Register.MatchRegisterReadOnly(invocation, semanticModel, CancellationToken.None)?.Target.Name);
            Assert.AreEqual("RegisterReadOnly", DependencyProperty.Register.MatchAny(invocation, semanticModel, CancellationToken.None)?.Target.Name);

            invocation = syntaxTree.FindInvocation("GetValue");
            Assert.AreEqual(null, DependencyProperty.Register.MatchRegisterReadOnly(invocation, semanticModel, CancellationToken.None));
            Assert.AreEqual(null, DependencyProperty.Register.MatchAny(invocation, semanticModel, CancellationToken.None));
        }

        [Test]
        public static void MatchRegisterAttached()
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

        public static void SetBar(FrameworkElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        public static int GetBar(FrameworkElement element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, Settings.Default.MetadataReferences);
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var invocation = syntaxTree.FindInvocation("RegisterAttached");
            Assert.AreEqual("RegisterAttached", DependencyProperty.Register.MatchRegisterAttached(invocation, semanticModel, CancellationToken.None)?.Target.Name);
            Assert.AreEqual("RegisterAttached", DependencyProperty.Register.MatchAny(invocation, semanticModel, CancellationToken.None)?.Target.Name);

            invocation = syntaxTree.FindInvocation("GetValue");
            Assert.AreEqual(null, DependencyProperty.Register.MatchRegisterAttached(invocation, semanticModel, CancellationToken.None));
            Assert.AreEqual(null, DependencyProperty.Register.MatchAny(invocation, semanticModel, CancellationToken.None));
        }

        [Test]
        public static void MatchRegisterAttachedFullyQualified()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace N
{
    public static class Foo
    {
        public static readonly System.Windows.DependencyProperty BarProperty = System.Windows.DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new System.Windows.PropertyMetadata(default(int)));

        public static void SetBar(System.Windows.DependencyObject element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        public static int GetBar(System.Windows.DependencyObject element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}");
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, Settings.Default.MetadataReferences);
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var invocation = syntaxTree.FindInvocation("RegisterAttached");
            Assert.AreEqual("RegisterAttached", DependencyProperty.Register.MatchRegisterAttached(invocation, semanticModel, CancellationToken.None)?.Target.Name);
            Assert.AreEqual("RegisterAttached", DependencyProperty.Register.MatchAny(invocation, semanticModel, CancellationToken.None)?.Target.Name);

            invocation = syntaxTree.FindInvocation("GetValue");
            Assert.AreEqual(null, DependencyProperty.Register.MatchRegisterAttached(invocation, semanticModel, CancellationToken.None));
            Assert.AreEqual(null, DependencyProperty.Register.MatchAny(invocation, semanticModel, CancellationToken.None));
        }

        [Test]
        public static void MatchRegisterAttachedReadOnly()
        {
            var code = @"
namespace N
{
    using System.Windows;

    public static class Foo
    {
        private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

            public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

        public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarPropertyKey, value);

        public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);
    }
}";
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, Settings.Default.MetadataReferences);
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var invocation = syntaxTree.FindInvocation("RegisterAttachedReadOnly");
            Assert.AreEqual("RegisterAttachedReadOnly", DependencyProperty.Register.MatchRegisterAttachedReadOnly(invocation, semanticModel, CancellationToken.None)?.Target.Name);
            Assert.AreEqual("RegisterAttachedReadOnly", DependencyProperty.Register.MatchAny(invocation, semanticModel, CancellationToken.None)?.Target.Name);

            invocation = syntaxTree.FindInvocation("GetValue");
            Assert.AreEqual(null, DependencyProperty.Register.MatchRegisterAttachedReadOnly(invocation, semanticModel, CancellationToken.None));
            Assert.AreEqual(null, DependencyProperty.Register.MatchAny(invocation, semanticModel, CancellationToken.None));
        }
    }
}
