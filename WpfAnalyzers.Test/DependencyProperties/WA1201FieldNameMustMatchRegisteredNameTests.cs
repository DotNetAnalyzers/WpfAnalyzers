namespace WpfAnalyzers.Test.DependencyProperties
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;

    using NUnit.Framework;

    using WpfAnalyzers.DependencyProperties;

    public class WA1201FieldNameMustMatchRegisteredNameTests : CodeFixVerifier
    {
        [TestCase("\"Bar\"")]
        [TestCase("nameof(Bar)")]
        [TestCase("nameof(FooControl.Bar)")]
        public async Task HappyPath(string nameof)
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(Bar),
        typeof(int),
        typeof(FooControl),
        new PropertyMetadata(default(int)));

    public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;
    
    public int Bar
    {
        get { return (int) GetValue(BarProperty); }
        set { SetValue(BarProperty, value); }
    }
}";
            testCode = testCode.Replace("nameof(Bar)", nameof);
            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task WhenNotMatching()
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    private static readonly DependencyPropertyKey ErrorPropertyKey = DependencyProperty.RegisterReadOnly(
        ""Bar"",
        typeof(int),
        typeof(FooControl),
        new PropertyMetadata(default(int)));

    public static readonly DependencyProperty BarProperty = ErrorPropertyKey.DependencyProperty;

    public int Bar
    {
        get { return (int) GetValue(BarProperty); }
        set { SetValue(ErrorPropertyKey, value); }
    }
}";

            var expected = this.CSharpDiagnostic().WithLocation(7, 51).WithArguments("ErrorPropertyKey", "Bar");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);

            var fixedCode = @"
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
        get { return (int) GetValue(BarProperty); }
        set { SetValue(BarPropertyKey, value); }
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new WA1201FieldNameMustMatchRegisteredName();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new RenameFieldCodeFixProvider();
        }
    }
}