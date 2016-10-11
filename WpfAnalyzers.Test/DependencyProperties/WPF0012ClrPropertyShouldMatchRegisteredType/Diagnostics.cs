namespace WpfAnalyzers.Test.DependencyProperties.WPF0012ClrPropertyShouldMatchRegisteredType
{
    using System.Threading.Tasks;

    using NUnit.Framework;

    using WpfAnalyzers.DependencyProperties;

    internal class Diagnostics : DiagnosticVerifier<WPF0012ClrPropertyShouldMatchRegisteredType>
    {
        [TestCase("double")]
        [TestCase("int?")]
        [TestCase("Nullable<int>")]
        [TestCase("ObservableCollection<int>")]
        public async Task DependencyProperty(string typeName)
        {
            var testCode = @"
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
        nameof(Bar),
        typeof(int),
        typeof(FooControl),
        new PropertyMetadata(default(int)));

    public double Bar
    {
        get { return (double)GetValue(BarProperty); }
        set { SetValue(BarProperty, value); }
    }
}";
            testCode = testCode.AssertReplace("double", typeName);
            var expected = this.CSharpDiagnostic().WithLocation(15, 12).WithArguments("FooControl.Bar", "int");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);
        }

        [Test]
        public async Task DependencyPropertyWithThis()
        {
            var testCode = @"
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            ""Bar"", 
            typeof(int), 
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public double Bar
        {
            get { return (double)this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
        }
    }";

            var expected = this.CSharpDiagnostic().WithLocation(13, 16).WithArguments("FooControl.Bar", "int");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);
        }

        [Test]
        public async Task DependencyPropertyAddOwner()
        {
            var part1 = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    public static readonly DependencyProperty BarProperty = Foo.BarProperty.AddOwner(typeof(FooControl));

    public double Bar
    {
        get { return (double) this.GetValue(BarProperty); }
        set { this.SetValue(BarProperty, value); }
    }
}";

            var part2 = @"
using System.Windows;

public static class Foo
{
    public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
        ""Bar"",
        typeof(int), 
        typeof(Foo), 
        new FrameworkPropertyMetadata(
            default(int), 
            FrameworkPropertyMetadataOptions.Inherits));

    public static void SetBar(DependencyObject element, int value)
    {
        element.SetValue(BarProperty, value);
    }

    public static int GetBar(DependencyObject element)
    {
        return (int) element.GetValue(BarProperty);
    }
}";

            DiagnosticResult expected = this.CSharpDiagnostic().WithLocation(9, 12).WithArguments("FooControl.Bar", "int");
            await this.VerifyCSharpDiagnosticAsync(new[] { part1, part2 }, expected).ConfigureAwait(false);
        }

        [Test]
        public async Task ReadOnlyDependencyProperty()
        {
            var testCode = @"
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

        public double Bar
        {
            get { return (double)this.GetValue(BarProperty); }
            protected set { this.SetValue(BarPropertyKey, value); }
        }
    }";

            var expected = this.CSharpDiagnostic().WithLocation(15, 16).WithArguments("FooControl.Bar", "int");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);
        }
    }
}