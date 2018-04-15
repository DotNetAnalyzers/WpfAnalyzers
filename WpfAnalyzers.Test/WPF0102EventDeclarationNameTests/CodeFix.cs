namespace WpfAnalyzers.Test.WPF0102EventDeclarationNameTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new RoutedEventEventDeclarationAnalyzer();
        private static readonly CodeFixProvider Fix = new RenameMemberCodeFixProvider();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("WPF0102");

        [Test]
        public void Message()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the ValueChanged event</summary>
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
            ""ValueChanged"",
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(FooControl));

        public event RoutedEventHandler ↓WrongName
        {
            add { this.AddHandler(ValueChangedEvent, value); }
            remove { this.RemoveHandler(ValueChangedEvent, value); }
        }
    }
}";

            var expectedDiagnostic = ExpectedDiagnostic.Create(
                "WPF0102",
                "Rename to: 'ValueChanged'.");
            AnalyzerAssert.Diagnostics(Analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void EventManagerRegisterRoutedEvent()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the ValueChanged event</summary>
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
            ""ValueChanged"",
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(FooControl));

        public event RoutedEventHandler ↓WrongName
        {
            add { this.AddHandler(ValueChangedEvent, value); }
            remove { this.RemoveHandler(ValueChangedEvent, value); }
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the ValueChanged event</summary>
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
            ""ValueChanged"",
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(FooControl));

        public event RoutedEventHandler ValueChanged
        {
            add { this.AddHandler(ValueChangedEvent, value); }
            remove { this.RemoveHandler(ValueChangedEvent, value); }
        }
    }
}";

            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Explicit("C#7")]
        [Test]
        public void EventManagerRegisterRoutedEventExpressionBodies()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the ValueChanged event</summary>
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
            ""ValueChanged"",
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(FooControl));

        public event RoutedEventHandler WrongName
        {
            add => this.AddHandler(ValueChangedEvent, value);
            remove => this.RemoveHandler(ValueChangedEvent, value);
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the ValueChanged event</summary>
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
            ""ValueChanged"",
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(FooControl));

        public event RoutedEventHandler ValueChanged
        {
            add => this.AddHandler(ValueChangedEvent, value);
            remove => this.RemoveHandler(ValueChangedEvent, value);
        }
    }
}";

            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode, allowCompilationErrors: AllowCompilationErrors.Yes);
        }
    }
}
