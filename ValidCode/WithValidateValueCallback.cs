namespace ValidCode
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
