// ReSharper disable All
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace ValidCode.DependencyProperties
{
    using System.Windows;
    using System.Windows.Media;

    public class FooControl1 : FrameworkElement
    {
        /// <summary>Identifies the <see cref="DoubleValue"/> dependency property.</summary>
        public static readonly DependencyProperty DoubleValueProperty = DependencyProperty.Register(
            nameof(DoubleValue),
            typeof(double),
            typeof(FooControl1));

        /// <summary>Identifies the <see cref="IntValue"/> dependency property.</summary>
        public static readonly DependencyProperty IntValueProperty = DependencyProperty.Register(
            nameof(IntValue),
            typeof(int),
            typeof(FooControl1),
            new FrameworkPropertyMetadata(
                default(int),
                FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure,
                OnIntValueChanged,
                CoerceIntValue),
            ValidateIntValue);

        /// <summary>Identifies the <see cref="Bar"/> dependency property.</summary>
        public static readonly DependencyProperty BarProperty = Foo1.BarProperty.AddOwner(typeof(FooControl1));

#pragma warning disable WPF0150 // Use nameof().
        private static readonly DependencyPropertyKey ReadOnlyValuePropertyKey = DependencyProperty.RegisterReadOnly(
            "ReadOnlyValue",
            typeof(string),
            typeof(FooControl1),
            new PropertyMetadata(default(string)));
#pragma warning restore WPF0150 // Use nameof().

        /// <summary>Identifies the <see cref="ReadOnlyValue"/> dependency property.</summary>
        public static readonly DependencyProperty ReadOnlyValueProperty = ReadOnlyValuePropertyKey.DependencyProperty;

        /// <summary>Identifies the <see cref="Brush"/> dependency property.</summary>
        public static readonly DependencyProperty BrushProperty = DependencyProperty.Register(
            nameof(Brush),
            typeof(Brush),
            typeof(FooControl1),
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
            d.SetValue(ReadOnlyValuePropertyKey, "abc");
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
            if (value is int i)
            {
                return i > 0;
            }

            return false;
        }
    }
}
