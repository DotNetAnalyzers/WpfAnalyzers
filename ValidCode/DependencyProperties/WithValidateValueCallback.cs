namespace ValidCode.DependencyProperties
{
    using System.Windows;

    public class WithValidateValueCallback : DependencyObject
    {
        /// <summary>Identifies the <see cref="Value1"/> dependency property.</summary>
        public static readonly DependencyProperty Value1Property = DependencyProperty.Register(
            nameof(Value1),
            typeof(int),
            typeof(WithValidateValueCallback),
            new PropertyMetadata(default(int)),
            o => (int)o > 0);

        /// <summary>Identifies the <see cref="Value2"/> dependency property.</summary>
        public static readonly DependencyProperty Value2Property = DependencyProperty.Register(
            "Value2",
            typeof(int),
            typeof(WithValidateValueCallback),
            new PropertyMetadata(default(int)),
            ValidateValue2);

        /// <summary>Identifies the <see cref="Value3"/> dependency property.</summary>
        public static readonly DependencyProperty Value3Property = DependencyProperty.Register(
            nameof(Value3),
            typeof(int),
            typeof(WithValidateValueCallback),
            new PropertyMetadata(default(int)),
            ValidateGreaterThanZero);

        /// <summary>Identifies the <see cref="Value4"/> dependency property.</summary>
        public static readonly DependencyProperty Value4Property = DependencyProperty.Register(
            nameof(Value4),
            typeof(int),
            typeof(WithValidateValueCallback),
            new PropertyMetadata(default(int)),
            ValidateGreaterThanZero);

        /// <summary>Identifies the <see cref="Value5"/> dependency property.</summary>
        public static readonly DependencyProperty Value5Property = DependencyProperty.Register(
            nameof(Value5),
            typeof(double),
            typeof(WithValidateValueCallback),
            new PropertyMetadata(default(double)),
            CommonValidation.ValidateDoubleIsGreaterThanZero);

        /// <summary>Identifies the <see cref="Value6"/> dependency property.</summary>
        public static readonly DependencyProperty Value6Property = DependencyProperty.Register(
            nameof(Value6),
            typeof(double),
            typeof(WithValidateValueCallback),
            new PropertyMetadata(default(double)),
            CommonValidation.ValidateDoubleIsGreaterThanZero);

        public static readonly DependencyProperty Value7Property = DependencyProperty.RegisterAttached(
            "Value7",
            typeof(double),
            typeof(WithValidateValueCallback),
            new PropertyMetadata(default(double)),
            CommonValidation.ValidateDoubleIsGreaterThanZero);

        public static readonly DependencyProperty Value8Property = DependencyProperty.RegisterAttached(
            "Value8",
            typeof(double),
            typeof(WithValidateValueCallback),
            new PropertyMetadata(default(double)),
            CommonValidation.ValidateDoubleIsGreaterThanZero);

        public int Value1
        {
            get => (int)this.GetValue(Value1Property);
            set => this.SetValue(Value1Property, value);
        }

        public int Value2
        {
            get => (int)this.GetValue(Value2Property);
            set => this.SetValue(Value2Property, value);
        }

        public int Value3
        {
            get => (int)this.GetValue(Value3Property);
            set => this.SetValue(Value3Property, value);
        }

        public int Value4
        {
            get => (int)this.GetValue(Value4Property);
            set => this.SetValue(Value4Property, value);
        }

        public double Value5
        {
            get => (double)this.GetValue(Value5Property);
            set => this.SetValue(Value5Property, value);
        }

        public double Value6
        {
            get => (double)this.GetValue(Value6Property);
            set => this.SetValue(Value6Property, value);
        }

        /// <summary>Helper for setting <see cref="Value7Property"/> on <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="DependencyObject"/> to set <see cref="Value7Property"/> on.</param>
        /// <param name="value">Value7 property value.</param>
        public static void SetValue7(DependencyObject element, double value)
        {
            element.SetValue(Value7Property, value);
        }

        /// <summary>Helper for getting <see cref="Value7Property"/> from <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="DependencyObject"/> to read <see cref="Value7Property"/> from.</param>
        /// <returns>Value7 property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static double GetValue7(DependencyObject element)
        {
            return (double)element.GetValue(Value7Property);
        }

        /// <summary>Helper for setting <see cref="Value8Property"/> on <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="DependencyObject"/> to set <see cref="Value8Property"/> on.</param>
        /// <param name="value">Value8 property value.</param>
        public static void SetValue8(DependencyObject element, double value)
        {
            element.SetValue(Value8Property, value);
        }

        /// <summary>Helper for getting <see cref="Value8Property"/> from <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="DependencyObject"/> to read <see cref="Value8Property"/> from.</param>
        /// <returns>Value8 property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static double GetValue8(DependencyObject element)
        {
            return (double)element.GetValue(Value8Property);
        }

        private static bool ValidateValue2(object value)
        {
            if (value is int i)
            {
                return i > 0;
            }

            return false;
        }

        private static bool ValidateGreaterThanZero(object value)
        {
            if (value is int i)
            {
                return i > 0;
            }

            return false;
        }
    }
}
