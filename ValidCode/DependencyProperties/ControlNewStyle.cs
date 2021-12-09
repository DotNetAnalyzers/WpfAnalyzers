namespace ValidCode.DependencyProperties
{
    using System.Windows;
    using System.Windows.Media;

    public class ControlNewStyle : FrameworkElement
    {
        /// <summary>Identifies the <see cref="DoubleValue"/> dependency property.</summary>
        public static readonly DependencyProperty DoubleValueProperty = DependencyProperty.Register(
            nameof(DoubleValue),
            typeof(double),
            typeof(ControlNewStyle));

        /// <summary>Identifies the <see cref="IntValue"/> dependency property.</summary>
        public static readonly DependencyProperty IntValueProperty = DependencyProperty.Register(
            nameof(IntValue),
            typeof(int),
            typeof(ControlNewStyle),
            new FrameworkPropertyMetadata(
                default(int),
                FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure,
                OnIntValueChanged,
                CoerceIntValue),
            ValidateIntValue);

        /// <summary>Identifies the <see cref="Bar"/> dependency property.</summary>
        public static readonly DependencyProperty BarProperty = AttachedProperties1.BarProperty.AddOwner(typeof(ControlNewStyle));

        private static readonly DependencyPropertyKey ReadOnlyValuePropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(ReadOnlyValue),
            typeof(string),
            typeof(ControlNewStyle),
            new PropertyMetadata(default(string)));

        /// <summary>Identifies the <see cref="ReadOnlyValue"/> dependency property.</summary>
        public static readonly DependencyProperty ReadOnlyValueProperty = ReadOnlyValuePropertyKey.DependencyProperty;

        /// <summary>Identifies the <see cref="Brush"/> dependency property.</summary>
        public static readonly DependencyProperty BrushProperty = DependencyProperty.Register(
            nameof(Brush),
            typeof(Brush),
            typeof(ControlNewStyle),
            new PropertyMetadata(default(Brush)));

        public double DoubleValue
        {
            get => (double)this.GetValue(DoubleValueProperty);
            set => this.SetValue(DoubleValueProperty, value);
        }

        public int IntValue
        {
            get => (int)this.GetValue(IntValueProperty);
            set => this.SetValue(IntValueProperty, value);
        }

        public bool Bar
        {
            get { return (bool)this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
        }

        public string? ReadOnlyValue
        {
            get => (string?)this.GetValue(ReadOnlyValueProperty);
            protected set => this.SetValue(ReadOnlyValuePropertyKey, value);
        }

        public Brush? Brush
        {
            get => (Brush?)this.GetValue(BrushProperty);
            set => this.SetValue(BrushProperty, value);
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

        private static object CoerceIntValue(DependencyObject d, object? baseValue)
        {
            // very strange stuff here, tests things.
#pragma warning disable WPF0041
            d.SetValue(BarProperty, baseValue);
#pragma warning restore WPF0041
            return d.GetValue(BarProperty);
        }

        private static bool ValidateIntValue(object? value)
        {
            if (value is int i)
            {
                return i > 0;
            }

            return false;
        }
    }

}
