namespace WpfAnalyzers.Test.DependencyProperties
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis.Diagnostics;

    using NUnit.Framework;

    using WpfAnalyzers.DependencyProperties;

    public class WPF0010DefaultValueMustMatchRegisteredTypeTests : DiagnosticVerifier
    {
        [Test]
        public async Task HappyPathNoMetadata()
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(double),
        typeof(FooControl));

    public double Value
    {
        get { return (double)this.GetValue(ValueProperty); }
        set { this.SetValue(ValueProperty, value); }
    }
}";
            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [TestCase("int", "new PropertyMetadata()")]
        [TestCase("int", "new FrameworkPropertyMetadata()")]
        [TestCase("int", "new PropertyMetadata(default(int))")]
        [TestCase("int", "new PropertyMetadata(1, OnValueChanged)")]
        [TestCase("int", "new PropertyMetadata(1)")]
        [TestCase("int?", "new PropertyMetadata(1)")]
        [TestCase("int?", "new PropertyMetadata(null)")]
        [TestCase("int?", "new PropertyMetadata(default(int?))")]
        [TestCase("Nullable<int>", "new PropertyMetadata(default(int?))")]
        [TestCase("int", "new PropertyMetadata(CreateDefaultValue())")]
        [TestCase("int", "new PropertyMetadata(CreateObjectValue())")]
        [TestCase("ObservableCollection<int>", "new PropertyMetadata(null)")]
        [TestCase("ObservableCollection<int>", "new PropertyMetadata(new ObservableCollection<int>())")]
        [TestCase("IEnumerable", "new PropertyMetadata(new ObservableCollection<int>())")]
        public async Task HappyPathWithMetdadata(string typeName, string metadata)
        {
            var testCode = @"
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(double),
        typeof(FooControl),
        new PropertyMetadata(1));

    public double Value
    {
        get { return (double)this.GetValue(ValueProperty); }
        set { this.SetValue(ValueProperty, value); }
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // nop
    }

    private static double CreateDefaultValue() => default(double);
    private static object CreateObjectValue() => default(double);
}";
            testCode = testCode.AssertReplace("new PropertyMetadata(1)", metadata)
                               .AssertReplace("double", typeName);
            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task HappyPathReadOnly()
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    private static readonly DependencyPropertyKey ValuePropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(Value),
        typeof(double),
        typeof(FooControl),
        new PropertyMetadata(1.0));

    public static readonly DependencyProperty ValueProperty = ValuePropertyKey.DependencyProperty;

    public double Value
    {
        get { return (double)this.GetValue(ValueProperty); }
        set { this.SetValue(ValuePropertyKey, value); }
    }
}";
            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task HappyPathAttached()
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

    public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarProperty, value);

    public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);
}";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task HappyPathAttachedReadonly()
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

    public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarPropertyKey, value);

    public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);
}";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [TestCase("int", "new PropertyMetadata(default(double))")]
        [TestCase("int", "new PropertyMetadata(0.0)")]
        [TestCase("double", "new PropertyMetadata(1)")]
        [TestCase("double", "new PropertyMetadata(null)")]
        [TestCase("double?", "new PropertyMetadata(1)")]
        [TestCase("System.Collections.ObjectModel.ObservableCollection<int>", "new PropertyMetadata(1)")]
        [TestCase("System.Collections.ObjectModel.ObservableCollection<int>", "new PropertyMetadata(new ObservableCollection<double>())")]
        public async Task WhenNotMatching(string typeName, string metadata)
        {
            var testCode = @"
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(double),
        typeof(FooControl),
        new PropertyMetadata(1));

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
                               .AssertReplace("new PropertyMetadata(1)", metadata);
            DiagnosticResult expected = this.CSharpDiagnostic().WithLocation(16, 30).WithArguments("ValueProperty", typeName);
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task WhenNotMatchingReadOnly()
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    private static readonly DependencyPropertyKey ValuePropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(Value),
        typeof(double),
        typeof(FooControl),
        new PropertyMetadata(""1.0""));

    public static readonly DependencyProperty ValueProperty = ValuePropertyKey.DependencyProperty;

    public double Value
    {
        get { return (double)this.GetValue(ValueProperty); }
        set { this.SetValue(ValuePropertyKey, value); }
    }
}";
            DiagnosticResult expected = this.CSharpDiagnostic().WithLocation(14, 30).WithArguments("ValuePropertyKey", "double");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task WhenNotMatchingAttached()
        {
            var testCode = @"
using System.Windows;

public static class Foo
{
    public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
        ""Bar"",
        typeof(int),
        typeof(Foo),
        new PropertyMetadata(1.0));

    public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarProperty, value);

    public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);
}";

            DiagnosticResult expected = this.CSharpDiagnostic().WithLocation(10, 30).WithArguments("BarProperty", "int");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task WhenNotMatchingAttachedReadonly()
        {
            var testCode = @"
using System.Windows;

public static class Foo
{
    private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
        ""Bar"",
        typeof(int),
        typeof(Foo),
        new PropertyMetadata(default(double)));

        public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

    public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarPropertyKey, value);

    public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);
}";

            DiagnosticResult expected = this.CSharpDiagnostic().WithLocation(10, 30).WithArguments("BarPropertyKey", "int");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        internal override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new WPF0010DefaultValueMustMatchRegisteredType();
        }
    }
}