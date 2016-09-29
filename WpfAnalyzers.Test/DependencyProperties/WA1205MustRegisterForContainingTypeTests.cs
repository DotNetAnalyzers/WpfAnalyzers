namespace WpfAnalyzers.Test.DependencyProperties
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis.Diagnostics;

    using NUnit.Framework;

    using WpfAnalyzers.DependencyProperties;

    public class WA1205MustRegisterForContainingTypeTests : DiagnosticVerifier
    {
        [TestCase("FooControl")]
        [TestCase("FooControl<T>")]
        public async Task HappyPath(string typeName)
        {
            var testCode = @"
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    // registering for an owner that is not containing type.
    public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
        nameof(Bar),
        typeof(int),
        typeof(FooControl),
        new PropertyMetadata(default(int)));

    public int Bar
    {
        get { return (int)this.GetValue(BarProperty); }
        set { this.SetValue(BarProperty, value); }
    }
}";
            testCode = testCode.AssertReplace("FooControl", typeName);
            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [TestCase("class BarControl", "typeof(BarControl)")]
        [TestCase("class BarControl<T>", "typeof(BarControl<int>)")]
        public async Task WhenNotOwner(string typeName, string typeofName)
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class BarControl : Control
{
}

public class FooControl : Control
{
    // registering for an owner that is not containing type.
    public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
        nameof(Bar),
        typeof(int),
        typeof(BarControl),
        new PropertyMetadata(default(int)));

    public int Bar
    {
        get { return (int)this.GetValue(BarProperty); }
        set { this.SetValue(BarProperty, value); }
    }
}";
            testCode = testCode.AssertReplace("class BarControl", typeName)
                               .AssertReplace("typeof(BarControl)", typeofName);
            var expected = this.CSharpDiagnostic().WithLocation(15, 16).WithArguments("FooControl.BarProperty", "FooControl");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task WhenNotOwnerReadOnly()
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class BarControl : Control
{
}

public class FooControl : Control
{
    // registering for an owner that is not containing type.
    private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterReadOnly(
        ""Bar"",
        typeof(int),
        typeof(BarControl),
        new PropertyMetadata(default(int)));

    public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

    public int Bar
    {
        get { return (int)this.GetValue(BarProperty); }
        protected set {  this.SetValue(BarPropertyKey, value);}
    }
}";

            var expected = this.CSharpDiagnostic().WithLocation(15, 16).WithArguments("FooControl.BarPropertyKey", "FooControl");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task WhenNotOwnerAttached()
        {
            var testCode = @"
using System.Windows;

public class Bar
{
}

public static class Foo
{
    public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
        ""Bar"",
        typeof(int),
        typeof(Bar),
        new PropertyMetadata(default(int)));

    public static void SetBar(DependencyObject element, int value)
    {
        element.SetValue(BarProperty, value);
    }

    public static int GetBar(DependencyObject element)
    {
        return (int)element.GetValue(BarProperty);
    }
}";

            var expected = this.CSharpDiagnostic().WithLocation(13, 16).WithArguments("Foo.BarProperty", "Foo");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task WhenNotOwnerAttachedReadonly()
        {
            var testCode = @"
using System.Windows;

public class Bar
{
}

public static class Foo
{
    private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
        ""Bar"",
        typeof(int),
        typeof(Bar),
        new PropertyMetadata(default(int)));

    public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

    public static void SetBar(DependencyObject element, int value)
    {
        element.SetValue(BarPropertyKey, value);
    }

    public static int GetBar(DependencyObject element)
    {
        return (int)element.GetValue(BarProperty);
    }
}";

            var expected = this.CSharpDiagnostic().WithLocation(13, 16).WithArguments("Foo.BarPropertyKey", "Foo");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new WA1205MustRegisterForContainingType();
        }
    }
}