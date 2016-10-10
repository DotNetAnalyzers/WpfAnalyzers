namespace WpfAnalyzers.Test.DependencyProperties
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis.Diagnostics;

    using NUnit.Framework;

    using WpfAnalyzers.DependencyProperties;

    public class WPF0014SetValueMustUseRegisteredTypeTests : DiagnosticVerifier
    {
        [Test]
        public async Task HappyPath()
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

    public int Bar
    {
        get { return (int)GetValue(BarProperty); }
        set { SetValue(BarProperty, value); }
    }

    public void Meh()
    {
        this.SetValue(BarProperty, 1);
        this.SetCurrentValue(BarProperty, 1);
    }
}";
            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task HappyPathObject()
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

    public int Bar
    {
        get { return (int)GetValue(BarProperty); }
        set { SetValue(BarProperty, value); }
    }

    public void Meh()
    {
        this.SetValue(BarProperty, (object)1);
        this.SetCurrentValue(BarProperty, (object)1);
    }
}";
            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task HappyPathObject2()
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

    public int Bar
    {
        get { return (int)GetValue(BarProperty); }
        set { SetValue(BarProperty, value); }
    }

    public void Meh()
    {
        var value = this.GetValue(BarProperty);
        this.SetValue(BarProperty, value);
        this.SetCurrentValue(BarProperty, value);
    }
}";
            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task HappyPathTextBox()
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public static class Foo
{
    public static void Bar()
    {
        var textBox = new TextBox();
        textBox.SetValue(TextBox.TextProperty, ""abc"");
    }
}";
            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [TestCase("SetValue(BarProperty, 1.0)")]
        [TestCase("SetCurrentValue(BarProperty, 1.0)")]
        [TestCase("this.SetValue(BarProperty, 1.0)")]
        [TestCase("this.SetCurrentValue(BarProperty, 1.0)")]
        [TestCase("SetValue(BarProperty, null)")]
        [TestCase("SetCurrentValue(BarProperty, null)")]
        [TestCase("SetValue(BarProperty, \"abc\")")]
        [TestCase("SetCurrentValue(BarProperty, \"abc\")")]
        public async Task WhenNotMatching(string setCall)
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

    public int Bar
    {
        get { return (int)GetValue(BarProperty); }
        set { SetValue(BarProperty, value); }
    }

    public void Meh()
    {
        this.SetValue(BarProperty, 1);
    }
}";
            testCode = testCode.AssertReplace("this.SetValue(BarProperty, 1)", setCall);
            var col = Regex.Match(testCode.Line(21), "BarProperty, +(?<value>[^ )])").Groups["value"].Index + 1;
            var method = setCall.Contains("SetValue")
                             ? "SetValue"
                             : "SetCurrentValue";
            var expected = this.CSharpDiagnostic().WithLocation(21, col).WithArguments(method, "int");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        [TestCase("1.0")]
        [TestCase("null")]
        [TestCase("\"abc\"")]
        public async Task WhenNotMatchingSetValueReadonly(string value)
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

    public int Bar
    {
        get { return (int)GetValue(BarProperty); }
        set { SetValue(BarPropertyKey, value); }
    }

    public void Meh()
    {
        this.SetValue(BarPropertyKey, <value>);
    }
}";
            testCode = testCode.AssertReplace("<value>", value);
            var expected = this.CSharpDiagnostic().WithLocation(23, 39).WithArguments("SetValue", "int");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task WhenNotMatchingAttached()
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

    public static int GetBar(FrameworkElement element)
    {
        return (int)element.GetValue(BarProperty);
    }

    public static void Meh(FrameworkElement element)
    {
        element.SetValue(BarProperty, 1.0);
    }
}";
            var expected = this.CSharpDiagnostic().WithLocation(26,39).WithArguments("SetValue", "int");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        [TestCase("SetValue")]
        [TestCase("SetCurrentValue")]
        public async Task WhenNotMatchingTextBoxTextProperty(string setMethod)
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public static class Foo
{
    public static void Bar()
    {
        var textBox = new TextBox();
        textBox.SetValue(TextBox.TextProperty, 1);
    }
}";
            testCode = testCode.AssertReplace("textBox.SetValue", $"textBox.{setMethod}");
            var col = Regex.Match(testCode.Line(10), "TextBox.TextProperty, +(?<value>[^ )])").Groups["value"].Index + 1;
            var expected = this.CSharpDiagnostic().WithLocation(10, col).WithArguments(setMethod, "string");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }


        [TestCase("SetValue")]
        [TestCase("SetCurrentValue")]
        public async Task WhenNotMatchingTextElementFontSizeProperty(string setMethod)
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
public static class Foo
{
    public static void Bar()
    {
        var textBox = new TextBox();
        textBox.SetValue(TextElement.FontSizeProperty, 1);
    }
}";
            testCode = testCode.AssertReplace("textBox.SetValue", $"textBox.{setMethod}");
            var col = Regex.Match(testCode.Line(10), "TextElement.FontSizeProperty, +(?<value>[^ )])").Groups["value"].Index + 1;
            var expected = this.CSharpDiagnostic().WithLocation(10, col).WithArguments(setMethod, "double");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        internal override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new WPF0014SetValueMustUseRegisteredType();
        }
    }
}