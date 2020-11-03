namespace WpfAnalyzers.Test
{
    using System.Threading;

    using Gu.Roslyn.Asserts;

    using Microsoft.CodeAnalysis.CSharp;

    using NUnit.Framework;

    public static class RegisterInvocationTests
    {
        [Test]
        public static void TryMatchRegister()
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
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var invocation = syntaxTree.FindInvocation("Register");
            Assert.AreEqual("Register",         RegisterInvocation.MatchRegister(invocation, semanticModel, CancellationToken.None)?.Target.Name);
            Assert.AreEqual("Register", RegisterInvocation.MatchRegisterAny(invocation, semanticModel, CancellationToken.None)?.Target.Name);

            invocation = syntaxTree.FindInvocation("GetValue");
            Assert.AreEqual(null, RegisterInvocation.MatchRegister(invocation, semanticModel, CancellationToken.None));
        }

        [Test]
        public static void TryMatchRegisterReadOnly()
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
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var invocation = syntaxTree.FindInvocation("RegisterReadOnly");
            Assert.AreEqual("RegisterReadOnly", RegisterInvocation.MatchRegisterReadOnly(invocation, semanticModel, CancellationToken.None)?.Target.Name);
            Assert.AreEqual("RegisterReadOnly", RegisterInvocation.MatchRegisterAny(invocation, semanticModel, CancellationToken.None)?.Target.Name);

            invocation = syntaxTree.FindInvocation("GetValue");
            Assert.AreEqual(null, RegisterInvocation.MatchRegisterReadOnly(invocation, semanticModel, CancellationToken.None));
        }

        [Test]
        public static void TryMatchRegisterAttached()
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
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var invocation = syntaxTree.FindInvocation("RegisterAttached");
            Assert.AreEqual("RegisterAttached", RegisterInvocation.MatchRegisterAttached(invocation, semanticModel, CancellationToken.None)?.Target.Name);
            Assert.AreEqual("RegisterAttached",         RegisterInvocation.MatchRegisterAny(invocation, semanticModel, CancellationToken.None)?.Target.Name);

            invocation = syntaxTree.FindInvocation("GetValue");
            Assert.AreEqual(null, RegisterInvocation.MatchRegisterAttached(invocation, semanticModel, CancellationToken.None));
        }

        [Test]
        public static void TryMatchRegisterAttachedFullyQualified()
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
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var invocation = syntaxTree.FindInvocation("RegisterAttached");
            Assert.AreEqual("RegisterAttached", RegisterInvocation.MatchRegisterAttached(invocation, semanticModel, CancellationToken.None)?.Target.Name);
            Assert.AreEqual("RegisterAttached", RegisterInvocation.MatchRegisterAny(invocation, semanticModel, CancellationToken.None)?.Target.Name);

            invocation = syntaxTree.FindInvocation("GetValue");
            Assert.AreEqual(null, RegisterInvocation.MatchRegisterAttached(invocation, semanticModel, CancellationToken.None));
        }

        [Test]
        public static void TryMatchRegisterAttachedReadOnly()
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
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var invocation = syntaxTree.FindInvocation("RegisterAttachedReadOnly");
            Assert.AreEqual("RegisterAttachedReadOnly", RegisterInvocation.MatchRegisterAttachedReadOnly(invocation, semanticModel, CancellationToken.None)?.Target.Name);
            Assert.AreEqual("RegisterAttachedReadOnly", RegisterInvocation.MatchRegisterAny(invocation, semanticModel, CancellationToken.None)?.Target.Name);

            invocation = syntaxTree.FindInvocation("GetValue");
            Assert.AreEqual(null, RegisterInvocation.MatchRegisterAttachedReadOnly(invocation, semanticModel, CancellationToken.None));
        }

        [Test]
        public static void TryGetAddOwnerCall()
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
