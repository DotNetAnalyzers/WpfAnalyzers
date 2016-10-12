namespace WpfAnalyzers.Test.DependencyProperties.WPF0015RegisteredOwnerTypeMustBeDependencyObject
{
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using WpfAnalyzers.DependencyProperties;

    internal class Diagnostics : DiagnosticVerifier<WPF0015RegisteredOwnerTypeMustBeDependencyObject>
    {
        [Test]
        public async Task AttachedPropertyUsingRegister()
        {
            var testCode = @"
using System;
using System.Collections.ObjectModel;
using System.Windows;

public static class Foo
{
    public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
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

    public static void Meh(FrameworkElement element)
    {
        element.SetValue(BarProperty, 1.0);
    }
}";
            var expected = this.CSharpDiagnostic().WithLocation(11, 9).WithArguments("RegisterAttached");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);
        }


        [Test]
        public async Task DependencyPropertyAddOwner()
        {
            var part1 = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl
{
    public static readonly DependencyProperty BarProperty = Foo.BarProperty.AddOwner(typeof(FooControl));

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

            var expected = this.CSharpDiagnostic().WithLocation(7, 86).WithArguments("RegisterAttached");
            await this.VerifyCSharpDiagnosticAsync(new[] { part1, part2 }, expected).ConfigureAwait(false);
        }


        [Test]
        public async Task ReadOnlyAttachedProperty()
        {
            var testCode = @"
using System.Windows;

public static class Foo
{
    private static readonly DependencyPropertyKey ErrorKey = DependencyProperty.RegisterReadOnly(
        ""Bar"",
        typeof(int),
        typeof(Foo),
        new PropertyMetadata(default(int)));

    public static readonly DependencyProperty BarProperty = ErrorKey.DependencyProperty;

    public static void SetBar(DependencyObject element, int value)
    {
        element.SetValue(ErrorKey, value);
    }

    public static int GetBar(DependencyObject element)
    {
        return (int)element.GetValue(BarProperty);
    }
}";

            var expected = this.CSharpDiagnostic().WithLocation(9, 9).WithArguments("RegisterAttached");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);
        }
    }
}