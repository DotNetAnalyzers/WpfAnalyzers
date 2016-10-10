namespace WpfAnalyzers.Test.DependencyProperties.WPF0013ClrMethodMustMatchRegisteredType
{
    using System.Threading.Tasks;

    using NUnit.Framework;

    using WpfAnalyzers.DependencyProperties;

    internal class CodeFix : DiagnosticVerifier<WPF0013ClrMethodMustMatchRegisteredType>
    {
        [TestCase("double")]
        [TestCase("int?")]
        [TestCase("Nullable<int>")]
        [TestCase("ObservableCollection<int>")]
        public async Task AttachedPropertySetMethod(string typeName)
        {
            var testCode = @"
using System;
using System.Collections.ObjectModel;
using System.Windows;

public static class Foo
{
    public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
        ""Bar"",
        typeof(int),
        typeof(Foo),
        new PropertyMetadata(default(int)));

    public static void SetBar(FrameworkElement element, double value)
    {
        element.SetValue(BarProperty, value);
    }

    public static int GetBar(FrameworkElement element)
    {
        return (int)element.GetValue(BarProperty);
    }
}";
            testCode = testCode.AssertReplace("double", typeName);
            var expected = this.CSharpDiagnostic().WithLocation(14, 57).WithArguments("Value type", "int");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);
        }

        [Test]
        public async Task AttachedPropertySetMethodAsExtensionMethod()
        {
            var testCode = @"
using System.Windows;

public static class Foo
{
    public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
        ""Bar"",
        typeof(int),
        typeof(Foo),
        new PropertyMetadata(default(int)));

    public static void SetBar(this FrameworkElement element, double value)
    {
        element.SetValue(BarProperty, value);
    }

    public static int GetBar(this FrameworkElement element)
    {
        return (int)element.GetValue(BarProperty);
    }
}";

            var expected = this.CSharpDiagnostic().WithLocation(12, 62).WithArguments("Value type", "int");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);
        }

        [Test]
        public async Task ReadOnlyAttachedPropertySetMethod()
        {
            var testCode = @"
using System.Windows;

public static class Foo
{
    private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
        ""Bar"",
        typeof(int),
        typeof(Foo),
        new PropertyMetadata(default(int)));

        public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

    public static void SetBar(this FrameworkElement element, double value) => element.SetValue(BarPropertyKey, value);

    public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);
}";

            var expected = this.CSharpDiagnostic().WithLocation(14, 62).WithArguments("Value type", "int");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);
        }

        [TestCase("double")]
        [TestCase("int?")]
        [TestCase("Nullable<int>")]
        [TestCase("ObservableCollection<int>")]
        public async Task AttachedPropertyGetMethod(string typeName)
        {
            var testCode = @"
using System;
using System.Collections.ObjectModel;
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

    public static double GetBar(FrameworkElement element)
    {
        return (double)element.GetValue(BarProperty);
    }
}";
            testCode = testCode.AssertReplace("double", typeName);
            var expected = this.CSharpDiagnostic().WithLocation(19, 19).WithArguments("Return type", "int");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);
        }

        [Test]
        public async Task AttachedPropertyGetMethodAsExtensionMethod()
        {
            var testCode = @"
using System.Windows;

public static class Foo
{
    public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
        ""Bar"",
        typeof(int),
        typeof(Foo),
        new PropertyMetadata(default(int)));

    public static void SetBar(this FrameworkElement element, int value)
    {
        element.SetValue(BarProperty, value);
    }

    public static double GetBar(this FrameworkElement element)
    {
        return (double)element.GetValue(BarProperty);
    }
}";

            var expected = this.CSharpDiagnostic().WithLocation(17, 19).WithArguments("Return type", "int");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);
        }
    }
}