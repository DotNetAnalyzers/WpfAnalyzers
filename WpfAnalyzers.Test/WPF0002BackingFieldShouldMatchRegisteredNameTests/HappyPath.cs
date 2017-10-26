namespace WpfAnalyzers.Test.WPF0002BackingFieldShouldMatchRegisteredNameTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class HappyPath
    {
        private static readonly WPF0002BackingFieldShouldMatchRegisteredName Analyzer = new WPF0002BackingFieldShouldMatchRegisteredName();

        [TestCase("\"Bar\"")]
        [TestCase("nameof(Bar)")]
        [TestCase("nameof(FooControl.Bar)")]
        public void RegisterReadOnlyBackingFields(string nameof)
        {
            var testCode = @"
namespace RoslynSandbox
{
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
            get { return (int)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }
    }
}";
            testCode = testCode.AssertReplace("nameof(Bar)", nameof);
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [TestCase("\"Bar\"")]
        [TestCase("nameof(Bar)")]
        [TestCase("nameof(FooControl.Bar)")]
        public void RegisterReadOnlyBackingProperties(string nameof)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        private static DependencyPropertyKey BarPropertyKey { get; } = DependencyProperty.RegisterReadOnly(
            nameof(Bar),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public static DependencyProperty BarProperty { get; } = BarPropertyKey.DependencyProperty;
    
        public int Bar
        {
            get { return (int)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }
    }
}";
            testCode = testCode.AssertReplace("nameof(Bar)", nameof);
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void RegisterReadOnlyRepro()
        {
            var statusCode = @"
namespace RoslynSandbox
{
    public enum Status
    {
        Idle,
        Updating
    }
}";
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        internal static readonly DependencyPropertyKey StatusPropertyKey = DependencyProperty.RegisterReadOnly(
            ""Status"",
            typeof(Status),
            typeof(FooControl),
            new PropertyMetadata(Status.Idle, OnStatusChanged));

        internal static readonly DependencyProperty StatusProperty = StatusPropertyKey.DependencyProperty;

        internal Status Status
        {
            get { return (Status)this.GetValue(StatusProperty); }
            set { this.SetValue(StatusProperty, value); }
        }

        private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // nop
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, statusCode, testCode);
        }

        [Test]
        public void RegisterAttachedReadOnly()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public static class Foo
    {
        private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            ""Bar"",
            typeof(int),
            typeof(Foo),
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
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void IgnoresDependencyProperty()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty ErrorProperty = DependencyProperty.Register(
            ""Bar"",
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int)GetValue(ErrorProperty); }
            set { SetValue(ErrorProperty, value); }
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void IgnoresAttachedProperty()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty ErrorProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        public static void SetBar(DependencyObject element, int value)
        {
            element.SetValue(ErrorProperty, value);
        }

        public static int GetBar(DependencyObject element)
        {
            return (int)element.GetValue(ErrorProperty);
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }
    }
}