namespace WpfAnalyzers.Test.WPF0016DefaultValueIsSharedReferenceTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new PropertyMetadataAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0016DefaultValueIsSharedReferenceType);

        [TestCase("ObservableCollection<int>", "new PropertyMetadata(↓new ObservableCollection<int>())")]
        [TestCase("int[]", "new PropertyMetadata(↓new int[1])")]
        public static void DependencyProperty(string typeName, string metadata)
        {
            var code = @"
#pragma warning disable CS8019
namespace N
{
    using System;
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(FooControl),
            new PropertyMetadata(↓1));

        public double Value
        {
            get { return (double)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // nop
        }
    }
}".AssertReplace("double", typeName)
  .AssertReplace("new PropertyMetadata(↓1)", metadata);

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public static void ReadOnlyDependencyProperty()
        {
            var code = @"
namespace N
{
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        private static readonly DependencyPropertyKey ValuePropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Value),
            typeof(ObservableCollection<int>),
            typeof(FooControl),
            new PropertyMetadata(↓new ObservableCollection<int>()));

        public static readonly DependencyProperty ValueProperty = ValuePropertyKey.DependencyProperty;

        public ObservableCollection<int> Value
        {
            get { return (ObservableCollection<int>)this.GetValue(ValueProperty); }
            set { this.SetValue(ValuePropertyKey, value); }
        }
    }
}";
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public static void DependencyPropertyRegisterAttached()
        {
            var code = @"
namespace N
{
    using System.Collections.ObjectModel;
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(ObservableCollection<int>),
            typeof(Foo),
            new PropertyMetadata(↓new ObservableCollection<int>()));

        public static void SetBar(this FrameworkElement element, ObservableCollection<int> value) => element.SetValue(BarProperty, value);

        public static ObservableCollection<int> GetBar(this FrameworkElement element) => (ObservableCollection<int>)element.GetValue(BarProperty);
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public static void DependencyPropertyRegisterAttachedReadOnly()
        {
            var code = @"
namespace N
{
    using System.Collections.ObjectModel;
    using System.Windows;

    public static class Foo
    {
        private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            ""Bar"",
            typeof(ObservableCollection<int>),
            typeof(Foo),
            new PropertyMetadata(↓new ObservableCollection<int>()));

            public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

        public static void SetBar(this FrameworkElement element, ObservableCollection<int> value) => element.SetValue(BarPropertyKey, value);

        public static ObservableCollection<int> GetBar(this FrameworkElement element) => (ObservableCollection<int>)element.GetValue(BarProperty);
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
