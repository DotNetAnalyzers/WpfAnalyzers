﻿namespace WpfAnalyzers.Test.DependencyProperties.WPF0041SetMutableUsingSetCurrentValue
{
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    using NUnit.Framework;

    using WpfAnalyzers.DependencyProperties;

    internal class CodeFix : CodeFixVerifier<WPF0041SetMutableUsingSetCurrentValue, UseSetCurrentValueCodeFixProvider>
    {
        [TestCase("Bar = 1;")]
        [TestCase("this.Bar = 1;")]
        [TestCase("this.Bar = this.CreateValue();")]
        public async Task ClrProperty(string setExpression)
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
        get { return (int)this.GetValue(BarProperty); }
        set { this.SetValue(BarProperty, value); }
    }

    public void Meh()
    {
        Bar = 1;
    }

    private int CreateValue() => 4;
}";
            var right = setExpression.Split('=')[1].Trim(' ', ';');
            testCode = testCode.AssertReplace("Bar = 1;", setExpression);
            var expected = this.CSharpDiagnostic().WithLocation(21, 9).WithArguments("BarProperty", right);
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);

            var fixedCode = @"
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

    public void Meh()
    {
        this.SetCurrentValue(BarProperty, 1);
    }

    private int CreateValue() => 4;
}";
            var thisPrefix = setExpression.StartsWith("this.")
                                 ? "this."
                                 : "";
            fixedCode = fixedCode.AssertReplace("this.SetCurrentValue(BarProperty, 1);", $"{thisPrefix}SetCurrentValue(BarProperty, {right});");
            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        [TestCase("Bar = 1;", "SetCurrentValue(FooControl.BarProperty, (double)1);")]
        [TestCase("Bar = 1.0;", "SetCurrentValue(FooControl.BarProperty, 1.0);")]
        [TestCase("SetValue(FooControl.BarProperty, 1.0);", "SetCurrentValue(FooControl.BarProperty, 1.0);")]
        [TestCase("Bar = CreateValue();", "SetCurrentValue(FooControl.BarProperty, CreateValue());")]
        [TestCase("SetValue(FooControl.BarProperty, CreateValue());", "SetCurrentValue(FooControl.BarProperty, CreateValue());")]
        [TestCase("SetValue(FooControl.BarProperty, CreateObjectValue());", "SetCurrentValue(FooControl.BarProperty, CreateObjectValue());")]
        public async Task ClrPropertyFromOutside(string before, string after)
        {
            var fooControlCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
        ""Bar"",
        typeof(double),
        typeof(FooControl),
        new PropertyMetadata(default(double)));

    public double Bar
    {
        get { return (double)this.GetValue(BarProperty); }
        set { this.SetValue(BarProperty, value); }
    }
}";

            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public static class Foo
{
    private static readonly FooControl FooControl = new FooControl();

    public static void Meh()
    {
        FooControl.Bar = 1;
    }

    private static double CreateValue() => 4;
    private static object CreateObjectValue() => 4;
}";
            testCode = testCode.AssertReplace("FooControl.Bar = 1;", "FooControl." + before);
            var value = Regex.Match(after, @"SetCurrentValue\(FooControl\.BarProperty, (\(double\))?(?<value>.+)\);", RegexOptions.ExplicitCapture)
                             .Groups["value"].Value;
            var expected = this.CSharpDiagnostic().WithLocation(11, 9).WithArguments("FooControl.BarProperty", value);
            await this.VerifyCSharpDiagnosticAsync(new[] { testCode, fooControlCode }, new[] { expected }, CancellationToken.None).ConfigureAwait(false);

            var fixedCode = @"
using System.Windows;
using System.Windows.Controls;

public static class Foo
{
    private static readonly FooControl FooControl = new FooControl();

    public static void Meh()
    {
        FooControl.SetCurrentValue(FooControl.BarProperty, 1);
    }

    private static double CreateValue() => 4;
    private static object CreateObjectValue() => 4;
}";
            fixedCode = fixedCode.AssertReplace("FooControl.SetCurrentValue(FooControl.BarProperty, 1);", "FooControl." + after);
            await this.VerifyCSharpFixAsync(new[] { testCode, fooControlCode }, new[] { fixedCode, fooControlCode }).ConfigureAwait(false);
        }

        [Test]
        public async Task ClrPropertyObject()
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
        get { return (int)this.GetValue(BarProperty); }
        set { this.SetValue(BarProperty, value); }
    }

    public void Meh()
    {
        var value = GetValue(BarProperty);
        SetValue(BarProperty, value);
    }
}";

            var expected = this.CSharpDiagnostic().WithLocation(22, 9).WithArguments("BarProperty", "value");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);

            var fixedCode = @"
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

    public void Meh()
    {
        var value = GetValue(BarProperty);
        SetCurrentValue(BarProperty, value);
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        [Test]
        public async Task ClrPropertyWhenFieldNameIsNotMatching()
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : TextBox
{
    internal static readonly DependencyProperty BarProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(double),
        typeof(FooControl));

    internal double Value
    {
        get { return (double)this.GetValue(BarProperty); }
        set { this.SetValue(BarProperty, value); }
    }

    public void Bar()
    {
        this.Value = 1.0;
    }
}";
            var expected = this.CSharpDiagnostic().WithLocation(20, 9).WithArguments("BarProperty", "1.0");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);

            var fixedCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : TextBox
{
    internal static readonly DependencyProperty BarProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(double),
        typeof(FooControl));

    internal double Value
    {
        get { return (double)this.GetValue(BarProperty); }
        set { this.SetValue(BarProperty, value); }
    }

    public void Bar()
    {
        this.SetCurrentValue(BarProperty, 1.0);
    }
}";

            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        [Test]
        public async Task InternalClrProperty()
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : TextBox
{
    internal static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(double),
        typeof(FooControl));

    internal double Value
    {
        get { return (double)this.GetValue(ValueProperty); }
        set { this.SetValue(ValueProperty, value); }
    }

    public void Bar()
    {
        this.Value = 1.0;
    }
}";
            var expected = this.CSharpDiagnostic().WithLocation(20, 9).WithArguments("ValueProperty", "1.0");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);

            var fixedCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : TextBox
{
    internal static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(double),
        typeof(FooControl));

    internal double Value
    {
        get { return (double)this.GetValue(ValueProperty); }
        set { this.SetValue(ValueProperty, value); }
    }

    public void Bar()
    {
        this.SetCurrentValue(ValueProperty, 1.0);
    }
}";

            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        [Test]
        public async Task ClrPropertySetInGenericClass()
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl<T> : TextBox
{
    internal static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(double),
        typeof(FooControl<T>));

    internal double Value
    {
        get { return (double)this.GetValue(ValueProperty); }
        set { this.SetValue(ValueProperty, value); }
    }

    public void Bar()
    {
        this.Value = 1.0;
    }
}";
            var expected = this.CSharpDiagnostic().WithLocation(20, 9).WithArguments("ValueProperty", "1.0");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);

            var fixedCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl<T> : TextBox
{
    internal static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(double),
        typeof(FooControl<T>));

    internal double Value
    {
        get { return (double)this.GetValue(ValueProperty); }
        set { this.SetValue(ValueProperty, value); }
    }

    public void Bar()
    {
        this.SetCurrentValue(ValueProperty, 1.0);
    }
}";

            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        [Test]
        public async Task ClrPropertyWithGenericBaseClass()
        {
            var fooControlCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl<T> : TextBox
{
    internal static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(double),
        typeof(FooControl<T>));

    internal double Value
    {
        get { return (double)this.GetValue(ValueProperty); }
        set { this.SetValue(ValueProperty, value); }
    }
}";

            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class BarControl : FooControl<int>
{
    public void Bar()
    {
        this.Value = 1.0;
    }
}";
            var expected = this.CSharpDiagnostic().WithLocation(9, 9).WithArguments("ValueProperty", "1.0");
            await this.VerifyCSharpDiagnosticAsync(new[] { testCode, fooControlCode }, expected, CancellationToken.None).ConfigureAwait(false);

            var fixedCode = @"
using System.Windows;
using System.Windows.Controls;

public class BarControl : FooControl<int>
{
    public void Bar()
    {
        this.SetCurrentValue(ValueProperty, 1.0);
    }
}";

            await this.VerifyCSharpFixAsync(new[] { testCode, fooControlCode }, new[] { fixedCode, fooControlCode }).ConfigureAwait(false);
        }

        [Test]
        public async Task ClrPropertyOnGenericClass()
        {
            var fooControlCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl<T> : TextBox
{
    internal static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(double),
        typeof(FooControl<T>));

    internal double Value
    {
        get { return (double)this.GetValue(ValueProperty); }
        set { this.SetValue(ValueProperty, value); }
    }
}";
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class Foo
{
    private readonly FooControl<int> fooControl = new FooControl<int>();

    public void Bar()
    {
        fooControl.Value = 1.0;
    }
}";
            var expected = this.CSharpDiagnostic().WithLocation(11, 9).WithArguments("FooControl<int>.ValueProperty", "1.0");
            await this.VerifyCSharpDiagnosticAsync(new[] { testCode, fooControlCode }, expected, CancellationToken.None).ConfigureAwait(false);

            var fixedCode = @"
using System.Windows;
using System.Windows.Controls;

public class Foo
{
    private readonly FooControl<int> fooControl = new FooControl<int>();

    public void Bar()
    {
        fooControl.SetCurrentValue(FooControl<int>.ValueProperty, 1.0);
    }
}";

            await this.VerifyCSharpFixAsync(new[] { testCode, fooControlCode }, new[] { fixedCode, fooControlCode }).ConfigureAwait(false);
        }

        [Test]
        public async Task ClrPropertyWithImplicitCast()
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : TextBox
{
    internal static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(double),
        typeof(FooControl));

    internal double Value
    {
        get { return (double)this.GetValue(ValueProperty); }
        set { this.SetValue(ValueProperty, value); }
    }

    public void Bar()
    {
        this.Value = 1;
    }
}";
            var expected = this.CSharpDiagnostic().WithLocation(20, 9).WithArguments("ValueProperty", "1");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);

            var fixedCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : TextBox
{
    internal static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(double),
        typeof(FooControl));

    internal double Value
    {
        get { return (double)this.GetValue(ValueProperty); }
        set { this.SetValue(ValueProperty, value); }
    }

    public void Bar()
    {
        this.SetCurrentValue(ValueProperty, (double)1);
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        [Test]
        public async Task ClrPropertyInBaseclass()
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : TextBox
{
    public void Bar()
    {
        this.Text = ""abc"";
    }
}";
            var expected = this.CSharpDiagnostic().WithLocation(9, 9).WithArguments("TextProperty", "\"abc\"");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);

            var fixedCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : TextBox
{
    public void Bar()
    {
        this.SetCurrentValue(TextProperty, ""abc"");
    }
}";

            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        [Test]
        public async Task SetValueInBaseclass()
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : TextBox
{
    public void Bar()
    {
        this.SetValue(TextProperty, ""abc"");
    }
}";
            var expected = this.CSharpDiagnostic().WithLocation(9, 9).WithArguments("TextProperty", "\"abc\"");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);

            var fixedCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : TextBox
{
    public void Bar()
    {
        this.SetCurrentValue(TextProperty, ""abc"");
    }
}";

            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        [TestCase("SetValue(BarProperty, 1);")]
        [TestCase("this.SetValue(BarProperty, 1);")]
        [TestCase("this.SetValue(BarProperty, this.CreateValue());")]
        public async Task SetValue(string setExpression)
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
        get { return (int)this.GetValue(BarProperty); }
        set { this.SetValue(BarProperty, value); }
    }

    public void Meh()
    {
        SetValue(BarProperty, 1);
    }

    private int CreateValue() => 4;
}";
            var value = Regex.Match(setExpression, @"(this\.)?SetValue\(BarProperty, (?<value>.+)\);", RegexOptions.ExplicitCapture).Groups["value"].Value;
            testCode = testCode.AssertReplace("SetValue(BarProperty, 1);", setExpression);
            var expected = this.CSharpDiagnostic().WithLocation(21, 9).WithArguments("BarProperty", value);
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);

            var fixedCode = @"
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

    public void Meh()
    {
        this.SetCurrentValue(BarProperty, 1);
    }

    private int CreateValue() => 4;
}";
            var thisPrefix = setExpression.StartsWith("this.")
                                 ? "this."
                                 : "";
            fixedCode = fixedCode.AssertReplace("this.SetCurrentValue(BarProperty, 1);", $"{thisPrefix}SetCurrentValue(BarProperty, {value});");
            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        [TestCase(@"Text = ""1"";")]
        [TestCase("Text = CreateValue();")]
        [TestCase("Text = this.CreateValue();")]
        public async Task InheritedTextBoxTexUsingClrProperty(string setExpression)
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : TextBox
{
    public void Meh()
    {
        this.Text = ""1"";
    }

    private string CreateValue() => ""2"";
}";
            var right = setExpression.Split('=')[1].Trim(' ', ';');
            testCode = testCode.AssertReplace("Text = \"1\";", setExpression);
            var expected = this.CSharpDiagnostic().WithLocation(9, 9).WithArguments("TextProperty", right);
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);

            var fixedCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : TextBox
{
    public void Meh()
    {
        this.SetCurrentValue(TextProperty, ""1"");
    }

    private string CreateValue() => ""2"";
}";
            fixedCode = fixedCode.AssertReplace("this.SetCurrentValue(TextProperty, \"1\");", $"this.SetCurrentValue(TextProperty, {right});");
            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        [TestCase("this.")]
        [TestCase("")]
        public async Task TextBoxFieldTexUsingClrProperty(string thisExpression)
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class Foo
{
    private readonly TextBox textBox = new TextBox();

    public void Meh()
    {
        this.textBox.Text = ""1"";
    }
}";

            testCode = testCode.AssertReplace("this.", thisExpression);
            var expected = this.CSharpDiagnostic().WithLocation(11, 9).WithArguments("TextBox.TextProperty", "\"1\"");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);

            var fixedCode = @"
using System.Windows;
using System.Windows.Controls;

public class Foo
{
    private readonly TextBox textBox = new TextBox();

    public void Meh()
    {
        this.textBox.SetCurrentValue(TextBox.TextProperty, ""1"");
    }
}";
            fixedCode = fixedCode.AssertReplace("this.", thisExpression);
            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        [Test]
        public async Task SetValueInLambda()
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

    public FooControl()
    {
        this.Loaded += (sender, args) =>
        {
            this.SetValue(BarProperty, 1);
        };
    }

    public int Bar
    {
        get { return (int)this.GetValue(BarProperty); }
        set { this.SetValue(BarProperty, value); }
    }
}";
            var expected = this.CSharpDiagnostic().WithLocation(17, 13).WithArguments("BarProperty", "1");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);

            var fixedCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
        ""Bar"",
        typeof(int),
        typeof(FooControl),
        new PropertyMetadata(default(int)));

    public FooControl()
    {
        this.Loaded += (sender, args) =>
        {
            this.SetCurrentValue(BarProperty, 1);
        };
    }

    public int Bar
    {
        get { return (int)this.GetValue(BarProperty); }
        set { this.SetValue(BarProperty, value); }
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }
    }
}