namespace WpfAnalyzers.Test.WPF0042AvoidSideEffectsInClrAccessorsTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class Diagnostics
    {
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
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            nameof(Bar),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public static readonly DependencyProperty OtherProperty = DependencyProperty.Register(
            ""Other"",
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public int Bar
        {
            get
            {
                ↓SideEffect(); 
                return (int)this.GetValue(BarProperty); 
            }
            set { this.SetValue(OtherProperty, value); }
        }

        private void SideEffect()
        {
        }
    }
}";
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                "WPF0042",
                "Avoid side effects in CLR accessors.");
            AnalyzerAssert.Diagnostics<WPF0042AvoidSideEffectsInClrAccessors>(expectedDiagnostic, testCode);
        }

        [Test]
        public void DependencyPropertySideEffectInGetter()
        {
            var testCode = @"
namespace RoslynSandbox
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

        public static readonly DependencyProperty OtherProperty = DependencyProperty.Register(
            ""Other"",
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public int Bar
        {
            get
            {
                ↓SideEffect(); 
                return (int)this.GetValue(BarProperty); 
            }
            set { this.SetValue(OtherProperty, value); }
        }

        private void SideEffect()
        {
        }
    }
}";
            AnalyzerAssert.Diagnostics<WPF0042AvoidSideEffectsInClrAccessors>(testCode);
        }

        [Test]
        public void DependencyPropertySideEffectInSetter()
        {
            var testCode = @"
namespace RoslynSandbox
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

        public static readonly DependencyProperty OtherProperty = DependencyProperty.Register(
            ""Other"",
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public int Bar
        {
            get
            {
                return (int)this.GetValue(BarProperty); 
            }
            set 
            {
                this.SetValue(OtherProperty, value);
                ↓SideEffect(); 
            }
        }

        private static void SideEffect()
        {
        }
    }
}";

            AnalyzerAssert.Diagnostics<WPF0042AvoidSideEffectsInClrAccessors>(testCode);
        }

        [Test]
        public void ReadOnlyDependencyProperty()
        {
            var testCode = @"
namespace RoslynSandbox
{
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
            protected set 
            { 
                this.SetValue(BarPropertyKey, value); 
                ↓SideEffect(); 
            }
        }

        private void SideEffect()
        {
        }
    }
}";

            AnalyzerAssert.Diagnostics<WPF0042AvoidSideEffectsInClrAccessors>(testCode);
        }

        [Test]
        public void DependencyPropertyRegisterAttachedWithSideEffectInSetMethod()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(1));

        public static void SetBar(this FrameworkElement element, int value)
        {
            ↓SideEffect(); 
            element.SetValue(BarProperty, value);
        }

        public static int GetBar(this FrameworkElement element)
        {
            return (int) element.GetValue(BarProperty);
        }

        private static void SideEffect()
        {
        }
    }
}";

            AnalyzerAssert.Diagnostics<WPF0042AvoidSideEffectsInClrAccessors>(testCode);
        }

        [Test]
        public void DependencyPropertyRegisterAttachedWithSideEffectInGetMethod()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(1));

        public static void SetBar(this FrameworkElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        public static int GetBar(this FrameworkElement element)
        {
            ↓SideEffect(); 
            return (int) element.GetValue(BarProperty);
        }

        private static void SideEffect()
        {
        }
    }
}";

            AnalyzerAssert.Diagnostics<WPF0042AvoidSideEffectsInClrAccessors>(testCode);
        }

        [Test]
        public void DependencyPropertyRegisterAttachedReadOnlyWithSideEffectInSetMethod()
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

        public static void SetBar(this FrameworkElement element, int value)
        {
            ↓SideEffect(); 
            element.SetValue(BarPropertyKey, value);
        }

        public static int GetBar(this FrameworkElement element)
        {
            return (int) element.GetValue(BarProperty);
        }

        private static void SideEffect()
        {
        }
    }
}";

            AnalyzerAssert.Diagnostics<WPF0042AvoidSideEffectsInClrAccessors>(testCode);
        }

        [Test]
        public void DependencyPropertyRegisterAttachedReadOnlyWithSideEffectInGetMethod()
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


        public static void SetBar(this FrameworkElement element, int value)
        {
            element.SetValue(BarPropertyKey, value);
        }

        public static int GetBar(this FrameworkElement element)
        {
            ↓SideEffect(); 
            return (int) element.GetValue(BarProperty);
        }

        private static void SideEffect()
        {
        }
    }
}";

            AnalyzerAssert.Diagnostics<WPF0042AvoidSideEffectsInClrAccessors>(testCode);
        }
    }
}