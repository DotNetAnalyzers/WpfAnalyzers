namespace WpfAnalyzers.Test.WPF0036AvoidSideEffectsInClrAccessorsTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class Diagnostics
    {
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("WPF0036");

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
                "WPF0036",
                "Avoid side effects in CLR accessors.");
            AnalyzerAssert.Diagnostics<PropertyDeclarationAnalyzer>(expectedDiagnostic, testCode);
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
            AnalyzerAssert.Diagnostics<PropertyDeclarationAnalyzer>(ExpectedDiagnostic, testCode);
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

            AnalyzerAssert.Diagnostics<PropertyDeclarationAnalyzer>(ExpectedDiagnostic, testCode);
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

            AnalyzerAssert.Diagnostics<PropertyDeclarationAnalyzer>(ExpectedDiagnostic, testCode);
        }
    }
}
