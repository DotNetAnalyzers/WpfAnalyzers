namespace WpfAnalyzers.Test.WPF0092RegisterClassHandlerDelegateType;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class CodeFix
{
    private static readonly RoutedEventCallbackAnalyzer Analyzer = new();
    private static readonly UseCorrectDelegateFix Fix = new();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0092WrongDelegateType);

    [TestCase("new Action<object, RoutedEventArgs>((sender, e) => { })")]
    [TestCase("(Action<object, RoutedEventArgs>)((sender, e) => { })")]
    public static void Message(string expression)
    {
        var code = @"
namespace N;

using System;
using System.Windows;
using System.Windows.Controls;

public static class C
{
    static C()
    {
        EventManager.RegisterClassHandler(
            typeof(PasswordBox),
            PasswordBox.PasswordChangedEvent,
            ↓new Action<object, RoutedEventArgs>((sender, e) => { }));
    }
}".AssertReplace("new Action<object, RoutedEventArgs>((sender, e) => { })", expression);
        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("Use correct handler type"), code);
    }

    [Test]
    public static void ExplicitAction()
    {
        var before = @"
namespace N;

using System;
using System.Windows;
using System.Windows.Controls;

public static class C
{
    static C()
    {
        EventManager.RegisterClassHandler(
            typeof(PasswordBox),
            PasswordBox.PasswordChangedEvent,
            ↓new Action<object, RoutedEventArgs>((sender, e) => { }));
    }
}";

        var after = @"
namespace N;

using System;
using System.Windows;
using System.Windows.Controls;

public static class C
{
    static C()
    {
        EventManager.RegisterClassHandler(
            typeof(PasswordBox),
            PasswordBox.PasswordChangedEvent,
            new RoutedEventHandler((sender, e) => { }));
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void CastAction()
    {
        var code = @"
namespace N;

using System;
using System.Windows;
using System.Windows.Controls;

public static class C
{
    static C()
    {
        EventManager.RegisterClassHandler(
            typeof(PasswordBox),
            PasswordBox.PasswordChangedEvent,
            ↓(Action<object, RoutedEventArgs>)((sender, e) => { }));
    }
}";

        RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, code);
    }
}
