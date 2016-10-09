namespace WpfAnalyzers.Test.DependencyProperties
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;

    using NUnit.Framework;

    using WpfAnalyzers.DependencyProperties;

    public class WPF0041SetMutableUsingSetCurrentValueTests : CodeFixVerifier
    {
        [Test]
        public async Task SetValueHappyPathReadOnly()
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
        protected set { SetValue(BarPropertyKey, value); }
    }
    
    public int Baz { get; set; }

    public void Meh()
    {
        Bar = 1;
        this.Bar = 2;
        this.Bar = this.CreateValue();
        Baz = 5;
        var control = new FooControl();
        control.Bar = 6;
    }

    private int CreateValue() => 4;
}";
            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task SetValueHappyPathReadOnlyFromOutside()
        {
            var fooControlCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    internal static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterReadOnly(
        ""Bar"",
        typeof(int),
        typeof(FooControl),
        new PropertyMetadata(default(int)));

    public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

    public int Bar
    {
        get { return (int)GetValue(BarProperty); }
        internal set { SetValue(BarPropertyKey, value); }
    }
}";

            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public static class Foo
{
    public static void Meh()
    {
        var fooControl = new FooControl();
        fooControl.Bar = 1;
        fooControl.SetValue(FooControl.BarPropertyKey, 1);
        fooControl.Bar = CreateValue();
        fooControl.SetValue(FooControl.BarPropertyKey, CreateValue());
        fooControl.SetValue(FooControl.BarPropertyKey, CreateObjectValue());
    }

    private static int CreateValue() => 4;
    private static object CreateObjectValue() => 4;
}";
            await this.VerifyCSharpDiagnosticAsync(new[] { fooControlCode, testCode }, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task SetValueHappyPathReadOnlyThis()
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
        get { return (int)this.GetValue(BarProperty); }
        set { this.SetValue(BarPropertyKey, value); }
    }
    
    public int Baz { get; set; }

    public void Meh()
    {
        Bar = 1;
        this.Bar = 2;
        this.Bar = this.CreateValue();
        Baz = 5;
        var control = new FooControl();
        control.Bar = 6;
    }

    private int CreateValue() => 4;
}";
            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task IgnoredInObjectInitializer()
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public static class Foo
{
    public static void Bar()
    {
        var textBlock = new TextBlock
        {
            Text = ""abc"",
            VerticalAlignment = VerticalAlignment.Center,
            IsHitTestVisible = false
        };
    }
}";
            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task IgnoredInConstructor()
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(double),
        typeof(FooControl));

    public FooControl()
    {
        this.Value = 2;
    }

    public double Value
    {
        get { return (double)this.GetValue(ValueProperty); }
        set { this.SetValue(ValueProperty, value); }
    }
}";
            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [TestCase("Bar = 1;")]
        [TestCase("this.Bar = 1;")]
        [TestCase("this.Bar = this.CreateValue();")]
        public async Task WhenSettingMutableUsingClrProperty(string setExpression)
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

        [TestCase("fooControl.Bar = 1;", "fooControl.SetCurrentValue(FooControl.BarProperty, (double)1);")]
        [TestCase("fooControl.Bar = 1.0;", "fooControl.SetCurrentValue(FooControl.BarProperty, 1.0);")]
        [TestCase("fooControl.SetValue(FooControl.BarProperty, 1.0);", "fooControl.SetCurrentValue(FooControl.BarProperty, 1.0);")]
        [TestCase("fooControl.Bar = CreateValue();", "fooControl.SetCurrentValue(FooControl.BarProperty, CreateValue());")]
        [TestCase("fooControl.SetValue(FooControl.BarProperty, CreateValue());", "fooControl.SetCurrentValue(FooControl.BarProperty, CreateValue());")]
        [TestCase("fooControl.SetValue(FooControl.BarProperty, CreateObjectValue());", "fooControl.SetCurrentValue(FooControl.BarProperty, CreateObjectValue());")]
        public async Task WhenSettingMutableUsingClrPropertyFromOutside(string before, string after)
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
    public static void Meh()
    {
        var fooControl = new FooControl();
        fooControl.Bar = 1;
    }

    private static double CreateValue() => 4;
    private static object CreateObjectValue() => 4;
}";
            testCode = testCode.AssertReplace("fooControl.Bar = 1;", before);
            var value = Regex.Match(after, @"fooControl\.SetCurrentValue\(FooControl\.BarProperty, (\(double\))?(?<value>.+)\);", RegexOptions.ExplicitCapture)
                             .Groups["value"].Value;
            var expected = this.CSharpDiagnostic().WithLocation(10, 9).WithArguments("FooControl.BarProperty", value);
            await this.VerifyCSharpDiagnosticAsync(new[] { testCode, fooControlCode }, new[] { expected }, CancellationToken.None).ConfigureAwait(false);

            var fixedCode = @"
using System.Windows;
using System.Windows.Controls;

public static class Foo
{
    public static void Meh()
    {
        var fooControl = new FooControl();
        fooControl.SetCurrentValue(FooControl.BarProperty, 1);
    }

    private static double CreateValue() => 4;
    private static object CreateObjectValue() => 4;
}";
            fixedCode = fixedCode.AssertReplace("fooControl.SetCurrentValue(FooControl.BarProperty, 1);", after);
            await this.VerifyCSharpFixAsync(new[] { testCode, fooControlCode }, new[] { fixedCode, fooControlCode }).ConfigureAwait(false);
        }

        [Test]
        public async Task WhenSettingMutableUsingClrPropertyObject()
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
        public async Task WhenSettingInternalClrProperty()
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
        public async Task WhenSettingClrPropertyInGenericClass()
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
        public async Task WhenSettingClrPropertyInSubclassGenericClass()
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
}

public class BarControl : FooControl<int>
{
    public void Bar()
    {
        this.Value = 1.0;
    }
}";
            var expected = this.CSharpDiagnostic().WithLocation(23, 9).WithArguments("ValueProperty", "1.0");
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
}

public class BarControl : FooControl<int>
{
    public void Bar()
    {
        this.SetCurrentValue(ValueProperty, 1.0);
    }
}";

            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        [Test]
        public async Task WhenSettingClrPropertyOnGenericClass()
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
}

public static class Foo
{
    public static void Bar()
    {
        var fooControl = new FooControl<int>();
        fooControl.Value = 1.0;
    }
}";
            var expected = this.CSharpDiagnostic().WithLocation(24, 9).WithArguments("ValueProperty", "1.0");
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
}

public static class Foo
{
    public static void Bar()
    {
        var fooControl = new FooControl<int>();
        fooControl.SetCurrentValue(FooControl<int>.ValueProperty, 1.0);
    }
}";

            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        [Test]
        public async Task WhenSettingClrPropertyWithImplicitCast()
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
        public async Task WhenSettingClrPropertyInBaseclass()
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
        public async Task WhenSetValueForPropertyInBaseclass()
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
        public async Task WhenSettingMutableUsingSetValue(string setExpression)
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
        public async Task WhenSettingTextBoxTexUsingClrProperty(string setExpression)
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class Foo
{
    public void Meh()
    {
        var textBox = new TextBox();
        textBox.Text = ""1"";
    }

    private string CreateValue() => ""2"";
}";
            var right = setExpression.Split('=')[1].Trim(' ', ';');
            testCode = testCode.AssertReplace("Text = \"1\";", setExpression);
            var expected = this.CSharpDiagnostic().WithLocation(10, 9).WithArguments("TextProperty", right);
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);

            var fixedCode = @"
using System.Windows;
using System.Windows.Controls;

public class Foo
{
    public void Meh()
    {
        var textBox = new TextBox();
        textBox.SetCurrentValue(TextBox.TextProperty, ""1"");
    }

    private string CreateValue() => ""2"";
}";
            fixedCode = fixedCode.AssertReplace("textBox.SetCurrentValue(TextBox.TextProperty, \"1\");", $"textBox.SetCurrentValue(TextBox.TextProperty, {right});");
            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new WPF0041SetMutableUsingSetCurrentValue();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new UseSetCurrentValueCodeFixProvider();
        }
    }
}