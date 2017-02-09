namespace WpfAnalyzers.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis.Diagnostics;

    using NUnit.Framework;

    public class HappyPathWithAll : DiagnosticVerifier
    {
        private static readonly IEnumerable<DiagnosticAnalyzer> AllAnalyzers = typeof(
                WpfAnalyzers.DependencyProperties.WPF0041SetMutableUsingSetCurrentValue).Assembly.GetTypes()
                                                                                        .Where(typeof(DiagnosticAnalyzer).IsAssignableFrom)
                                                                                        .Select(t => (DiagnosticAnalyzer)Activator.CreateInstance(t));

        [Test]
        public void NotEmpty()
        {
            CollectionAssert.IsNotEmpty(this.GetCSharpDiagnosticAnalyzers());
            Assert.Pass($"Count: {this.GetCSharpDiagnosticAnalyzers().Count()}");
        }

        public override void IdMatches()
        {
            Assert.Pass();
        }

        ////[Explicit("Temporarily ignore")]
        [Test]
        public async Task SomewhatRealisticSample()
        {
            // this test just throws some random code at all analyzers
            var booleanBoxesCode = @"
internal static class BooleanBoxes
{
    internal static readonly object True = true;
    internal static readonly object False = false;

    internal static object Box(bool value)
    {
        return value
                    ? True
                    : False;
    }
}";

            var fooCode = @"
    using System.Windows;
    using System.Windows.Controls;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(bool),
            typeof(Foo),
            new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty OtherProperty = DependencyProperty.RegisterAttached(
            ""Other"",
            typeof(string),
            typeof(Foo),
            new FrameworkPropertyMetadata(
                ""abc"", 
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsArrange, 
                OnOtherChanged, 
                CoerceOther));

        private static readonly DependencyPropertyKey ReadOnlyPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            ""ReadOnly"",
            typeof(bool),
            typeof(Foo),
            new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty ReadOnlyProperty = ReadOnlyPropertyKey.DependencyProperty;

        public static void SetBar(FrameworkElement element, bool value)
        {
            element.SetValue(BarProperty, BooleanBoxes.Box(value));
        }

        public static bool GetBar(FrameworkElement element)
        {
            return (bool)element.GetValue(BarProperty);
        }

        public static void SetOther(this DependencyObject element, string value)
        {
            element.SetValue(OtherProperty, value);
        }

        [AttachedPropertyBrowsableForChildren(IncludeDescendants = false)]
        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static string GetOther(this DependencyObject element)
        {
            return (string)element.GetValue(OtherProperty);
        }

        private static void SetReadOnly(this Control element, bool value)
        {
            element.SetValue(ReadOnlyPropertyKey, value);
        }

        [AttachedPropertyBrowsableForChildren(IncludeDescendants = false)]
        [AttachedPropertyBrowsableForType(typeof(Control))]
        public static bool GetReadOnly(this Control element)
        {
            return (bool)element.GetValue(ReadOnlyProperty);
        }

        private static object CoerceOther(DependencyObject d, object basevalue)
        {
            // very strange stuff here, tests things.
#pragma warning disable WPF0041
            d.SetValue(OtherProperty, basevalue);
#pragma warning restore WPF0041
            return d.GetValue(BarProperty);
        }

        private static void OnOtherChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.SetCurrentValue(BarProperty, true);
            d.SetValue(ReadOnlyPropertyKey, true);
        }
    }";

            var fooControlCode = @"
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public class FooControl : FrameworkElement
    {
        public static readonly DependencyProperty DoubleValueProperty = DependencyProperty.Register(
            nameof(DoubleValue),
            typeof(double),
            typeof(FooControl));

        public static readonly DependencyProperty IntValueProperty = DependencyProperty.Register(
            nameof(IntValue),
            typeof(int),
            typeof(FooControl),
            new FrameworkPropertyMetadata(
                default(int),
                FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure,
                OnIntValueChanged,
                CoerceIntValue),
            IntValueValidateValue);

        public static readonly DependencyProperty BarProperty = Foo.BarProperty.AddOwner(typeof(FooControl));

        private static readonly DependencyPropertyKey ReadOnlyValuePropertyKey = DependencyProperty.RegisterReadOnly(
            ""ReadOnlyValue"",
            typeof(string),
            typeof(FooControl),
            new PropertyMetadata(default(string)));

        public static readonly DependencyProperty ReadOnlyValueProperty = ReadOnlyValuePropertyKey.DependencyProperty;

        public static readonly DependencyProperty BrushProperty = DependencyProperty.Register(
            nameof(Brush),
            typeof(Brush),
            typeof(FooControl),
            new PropertyMetadata(default(Brush)));

        public double DoubleValue
        {
            get { return (double)this.GetValue(DoubleValueProperty); }
            set { this.SetValue(DoubleValueProperty, value); }
        }

        public int IntValue
        {
            get { return (int)this.GetValue(IntValueProperty); }
            set { this.SetValue(IntValueProperty, value); }
        }

        public bool Bar
        {
            get { return (bool)this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
        }

        public string ReadOnlyValue
        {
            get
            {
                return (string)this.GetValue(ReadOnlyValueProperty);
            }
            protected set
            {
                this.SetValue(ReadOnlyValuePropertyKey, value);
            }
        }

        public Brush Brush
        {
            get { return (Brush)this.GetValue(BrushProperty); }
            set { this.SetValue(BrushProperty, value); }
        }

        public void UpdateBrush(Brush brush)
        {
            this.SetCurrentValue(BrushProperty, brush?.GetAsFrozen());
#pragma warning disable WPF0041
            this.SetValue(BrushProperty, brush?.GetAsFrozen());
#pragma warning restore WPF0041
        }

        public void Meh(DependencyProperty property, object value)
        {
            this.SetValue(property, value);
            this.SetCurrentValue(property, value);
        }

        private static void OnIntValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.SetCurrentValue(BarProperty, true);
            d.SetValue(ReadOnlyValuePropertyKey, ""abc"");
        }

        private static object CoerceIntValue(DependencyObject d, object basevalue)
        {
            // very strange stuff here, tests things.
#pragma warning disable WPF0041
            d.SetValue(BarProperty, basevalue);
#pragma warning restore WPF0041
            return d.GetValue(BarProperty);
        }

        private static bool IntValueValidateValue(object value)
        {
            return true;
        }
    }";
            await this.VerifyCSharpDiagnosticAsync(new[] { fooCode, fooControlCode, booleanBoxesCode }, EmptyDiagnosticResults).ConfigureAwait(false);
        }

        internal override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            return AllAnalyzers;
        }
    }
}
