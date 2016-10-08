namespace WpfAnalyzers.Test.DependencyProperties
{
    using System.Collections.Generic;
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
        set { SetValue(BarPropertyKey, value); }
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
        this.Bar = 1;
    }

    private int CreateValue() => 4;
}";
            var thisPrefix = setExpression.StartsWith("this.")
                                 ? "this."
                                 : "";
            fixedCode = fixedCode.AssertReplace("this.Bar = 1;", $"{thisPrefix}SetCurrentValue(BarProperty, {right});");
            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        [TestCase("Bar = 1;")]
        [TestCase("Bar = this.CreateValue();")]
        public async Task WhenSettingMutableUsingClrPropertyFromOutside(string setExpression)
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
}

public class Baz
{
    public void Meh()
    {
        var control = new FooControl();
        control.Bar = 1;
    }

    private int CreateValue() => 2;
}";
            var right = setExpression.Split('=')[1].Trim(' ', ';');
            testCode = testCode.AssertReplace("Bar = 1;", setExpression);
            var expected = this.CSharpDiagnostic().WithLocation(25, 9).WithArguments("BarProperty", right);
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
        get { return (int)GetValue(BarProperty); }
        set { SetValue(BarProperty, value); }
    }
}

public class Baz
{
    public void Meh()
    {
        var control = new FooControl();
        control.Bar = 1;
    }

    private int CreateValue() => 2;
}";
            fixedCode = fixedCode.AssertReplace("Bar = 1;", $"SetCurrentValue(FooControl.BarProperty, {right});");
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