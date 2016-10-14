namespace WpfAnalyzers.Test.DependencyProperties.WPF0016DefaultValueIsSharedReferenceType
{
    using System.Threading.Tasks;

    using NUnit.Framework;

    using WpfAnalyzers.DependencyProperties;

    internal class Diagnostics : DiagnosticVerifier<WPF0016DefaultValueIsSharedReferenceType>
    {
        [TestCase("ObservableCollection<int>", "new PropertyMetadata(↓new ObservableCollection<int>())")]
        [TestCase("int[]", "new PropertyMetadata(↓new int[1])")]
        public async Task DependencyProperty(string typeName, string metadata)
        {
            var testCode = @"
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(double),
        typeof(FooControl),
        new PropertyMetadata(↓1));

    public double Value
    {
        get { return (double)this.GetValue(ValueProperty); }
        set { this.SetValue(ValueProperty, value); }
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // nop
    }
}";
            testCode = testCode.AssertReplace("double", typeName)
                               .AssertReplace("new PropertyMetadata(↓1)", metadata);
            var expected = this.CSharpDiagnostic()
                               .WithLocationIndicated(ref testCode)
                               .WithMessage("Default value for 'FooControl.ValueProperty' is a reference type that will be shared among all instances.");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);
        }

        [Test]
        public async Task ReadOnlyDependencyProperty()
        {
            var testCode = @"
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    private static readonly DependencyPropertyKey ValuePropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(Value),
        typeof(ObservableCollection<int>),
        typeof(FooControl),
        new PropertyMetadata(↓new ObservableCollection<int>()));

    public static readonly DependencyProperty ValueProperty = ValuePropertyKey.DependencyProperty;

    public ObservableCollection<int> Value
    {
        get { return (ObservableCollection<int>)this.GetValue(ValueProperty); }
        set { this.SetValue(ValuePropertyKey, value); }
    }
}";
            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("FooControl.ValuePropertyKey", "double");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);
        }

        [Test]
        public async Task AttachedProperty()
        {
            var testCode = @"
using System;
using System.Collections.ObjectModel;
using System.Windows;

public static class Foo
{
    public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
        ""Bar"",
        typeof(ObservableCollection<int>),
        typeof(Foo),
        new PropertyMetadata(↓new ObservableCollection<int>()));

    public static void SetBar(this FrameworkElement element, ObservableCollection<int> value) => element.SetValue(BarProperty, value);

    public static ObservableCollection<int> GetBar(this FrameworkElement element) => (ObservableCollection<int>)element.GetValue(BarProperty);
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("Foo.BarProperty", "int");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);
        }

        [Test]
        public async Task ReadOnlyAttachedProperty()
        {
            var testCode = @"
using System;
using System.Collections.ObjectModel;
using System.Windows;

public static class Foo
{
    private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
        ""Bar"",
        typeof(ObservableCollection<int>),
        typeof(Foo),
        new PropertyMetadata(↓new ObservableCollection<int>()));

        public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

    public static void SetBar(this FrameworkElement element, ObservableCollection<int> value) => element.SetValue(BarPropertyKey, value);

    public static ObservableCollection<int> GetBar(this FrameworkElement element) => (ObservableCollection<int>)element.GetValue(BarProperty);
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("Foo.BarPropertyKey", "int");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);
        }
    }
}