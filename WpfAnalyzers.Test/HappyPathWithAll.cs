namespace WpfAnalyzers.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;

    using NUnit.Framework;

    public class HappyPathWithAll
    {
        private static readonly IReadOnlyList<DiagnosticAnalyzer> AllAnalyzers = typeof(KnownSymbol)
            .Assembly.GetTypes()
            .Where(typeof(DiagnosticAnalyzer).IsAssignableFrom)
            .Select(t => (DiagnosticAnalyzer)Activator.CreateInstance(t))
            .ToArray();

        private static readonly Solution Solution = CodeFactory.CreateSolution(
            CodeFactory.FindSolutionFile("WpfAnalyzers.sln"),
            AllAnalyzers,
            AnalyzerAssert.MetadataReferences);

        private static readonly Solution AnalyzerProjectSln = CodeFactory.CreateSolution(
            CodeFactory.FindProjectFile("WpfAnalyzers.Analyzers.csproj"),
            AllAnalyzers,
            AnalyzerAssert.MetadataReferences);

        [Test]
        public void NotEmpty()
        {
            CollectionAssert.IsNotEmpty(AllAnalyzers);
            Assert.Pass($"Count: {AllAnalyzers.Count}");
        }

        [TestCaseSource(nameof(AllAnalyzers))]
        public void PropertyChangedAnalyzersProject(DiagnosticAnalyzer analyzer)
        {
            AnalyzerAssert.Valid(analyzer, AnalyzerProjectSln);
        }

        [TestCaseSource(nameof(AllAnalyzers))]
        public void PropertyChangedAnalyzersSln(DiagnosticAnalyzer analyzer)
        {
            AnalyzerAssert.Valid(analyzer, Solution);
        }

        [TestCaseSource(nameof(AllAnalyzers))]
        public void SomewhatRealisticSample(DiagnosticAnalyzer analyzer)
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
        /// <summary>Identifies the <see cref=""Bar""/> dependency property.</summary>
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(bool),
            typeof(Foo),
            new PropertyMetadata(default(bool)));

        /// <summary>Identifies the <see cref=""Other""/> dependency property.</summary>
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

        /// <summary>Identifies the <see cref=""ReadOnlyProperty""/> dependency property.</summary>
        public static readonly DependencyProperty ReadOnlyProperty = ReadOnlyPropertyKey.DependencyProperty;

        /// <summary>
        /// Helper for setting Bar property on a FrameworkElement.
        /// </summary>
        /// <param name=""element"">FrameworkElement to set Bar property on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(FrameworkElement element, bool value)
        {
            element.SetValue(BarProperty, BooleanBoxes.Box(value));
        }

        /// <summary>
        /// Helper for reading Bar property from a FrameworkElement.
        /// </summary>
        /// <param name=""element"">FrameworkElement to read Bar property from.</param>
        /// <returns>Bar property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
        public static bool GetBar(FrameworkElement element)
        {
            return (bool)element.GetValue(BarProperty);
        }

        /// <summary>
        /// Helper for setting Other property on a DependencyObject.
        /// </summary>
        /// <param name=""element"">DependencyObject to set Other property on.</param>
        /// <param name=""value"">Other property value.</param>
        public static void SetOther(this DependencyObject element, string value)
        {
            element.SetValue(OtherProperty, value);
        }

        /// <summary>
        /// Helper for reading Other property from a DependencyObject.
        /// </summary>
        /// <param name=""element"">DependencyObject to read Other property from.</param>
        /// <returns>Bar property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static string GetOther(this DependencyObject element)
        {
            return (string)element.GetValue(OtherProperty);
        }

        private static void SetReadOnly(this Control element, bool value)
        {
            element.SetValue(ReadOnlyPropertyKey, value);
        }

        /// <summary>
        /// Helper for reading ReadOnly property from a Control.
        /// </summary>
        /// <param name=""element"">Control to read ReadOnly property from.</param>
        /// <returns>ReadOnly property value.</returns>
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
        /// <summary>Identifies the <see cref=""DoubleValue""/> dependency property.</summary>
        public static readonly DependencyProperty DoubleValueProperty = DependencyProperty.Register(
            nameof(DoubleValue),
            typeof(double),
            typeof(FooControl));

        /// <summary>Identifies the <see cref=""IntValue""/> dependency property.</summary>
        public static readonly DependencyProperty IntValueProperty = DependencyProperty.Register(
            nameof(IntValue),
            typeof(int),
            typeof(FooControl),
            new FrameworkPropertyMetadata(
                default(int),
                FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure,
                OnIntValueChanged,
                CoerceIntValue),
            ValidateIntValue);

        /// <summary>Identifies the <see cref=""Bar""/> dependency property.</summary>
        public static readonly DependencyProperty BarProperty = Foo.BarProperty.AddOwner(typeof(FooControl));

        private static readonly DependencyPropertyKey ReadOnlyValuePropertyKey = DependencyProperty.RegisterReadOnly(
            ""ReadOnlyValue"",
            typeof(string),
            typeof(FooControl),
            new PropertyMetadata(default(string)));

        /// <summary>Identifies the <see cref=""ReadOnlyValue""/> dependency property.</summary>
        public static readonly DependencyProperty ReadOnlyValueProperty = ReadOnlyValuePropertyKey.DependencyProperty;

        /// <summary>Identifies the <see cref=""Brush""/> dependency property.</summary>
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

        private static bool ValidateIntValue(object value)
        {
            return true;
        }
    }";
            AnalyzerAssert.Valid(analyzer, fooCode, fooControlCode, booleanBoxesCode);
        }
    }
}
