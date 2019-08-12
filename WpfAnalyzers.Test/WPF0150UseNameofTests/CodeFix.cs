namespace WpfAnalyzers.Test.WPF0150UseNameofTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly CodeFixProvider Fix = new UseNameofFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0150UseNameof);

        [Test]
        public static void RoutedCommand()
        {
            var before = @"
namespace N
{
    using System.Windows.Input;

    public static class Foo
    {
        public static readonly RoutedCommand Bar = new RoutedCommand(↓""Bar"", typeof(Foo));
    }
}";

            var after = @"
namespace N
{
    using System.Windows.Input;

    public static class Foo
    {
        public static readonly RoutedCommand Bar = new RoutedCommand(nameof(Bar), typeof(Foo));
    }
}";
            RoslynAssert.CodeFix(new RoutedCommandCreationAnalyzer(), Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void RoutedUICommand()
        {
            var before = @"
namespace N
{
    using System.Windows.Input;

    public static class Foo
    {
        public static readonly RoutedUICommand Bar = new RoutedUICommand(""Some text"", ↓""Bar"", typeof(Foo));
    }
}";

            var after = @"
namespace N
{
    using System.Windows.Input;

    public static class Foo
    {
        public static readonly RoutedUICommand Bar = new RoutedUICommand(""Some text"", nameof(Bar), typeof(Foo));
    }
}";
            RoslynAssert.CodeFix(new RoutedCommandCreationAnalyzer(), Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void DependencyPropertyRegister()
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(↓""Bar"", typeof(int), typeof(FooControl), new PropertyMetadata(default(int)));

        public int Bar
        {
            get => (int)GetValue(BarProperty);
            set => SetValue(BarProperty, value);
        }
    }
}";

            var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(nameof(Bar), typeof(int), typeof(FooControl), new PropertyMetadata(default(int)));

        public int Bar
        {
            get => (int)GetValue(BarProperty);
            set => SetValue(BarProperty, value);
        }
    }
}";
            RoslynAssert.CodeFix(new RegistrationAnalyzer(), Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void DependencyPropertyRegisterArgumentPerLine()
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            ↓""Bar"",
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public int Bar
        {
            get => (int)GetValue(BarProperty);
            set => SetValue(BarProperty, value);
        }
    }
}";

            var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            nameof(Bar),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public int Bar
        {
            get => (int)GetValue(BarProperty);
            set => SetValue(BarProperty, value);
        }
    }
}";
            RoslynAssert.CodeFix(new RegistrationAnalyzer(), Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void RegisterRoutedEvent()
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the <see cref=""ValueChanged""/> event</summary>
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
            ↓""ValueChanged"",
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

            var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the <see cref=""ValueChanged""/> event</summary>
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(ValueChanged),
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
            RoslynAssert.CodeFix(new RoutedEventBackingFieldOrPropertyAnalyzer(), Fix, ExpectedDiagnostic, before, after);
        }
    }
}
