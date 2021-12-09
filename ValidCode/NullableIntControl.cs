namespace ValidCode
{
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;

    public class NullableIntControl : Control
    {
        /// <summary>Identifies the <see cref="Number"/> dependency property.</summary>
        public static readonly DependencyProperty NumberProperty = DependencyProperty.Register(
            nameof(Number),
            typeof(int?),
            typeof(NullableIntControl),
            new PropertyMetadata(
                null,
                OnNumberChanged,
                CoerceNumber),
            ValidateNumber);

        static NullableIntControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NullableIntControl), new FrameworkPropertyMetadata(typeof(NullableIntControl)));
        }

        public int? Number
        {
            get => (int?)this.GetValue(NumberProperty);
            set => this.SetValue(NumberProperty, value);
        }

        protected void OnNumberChanged(int? oldValue, int? newValue)
        {
        }

        private static void OnNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(d))
            {
                return;
            }

            ((NullableIntControl)d).OnNumberChanged((int?)e.NewValue, (int?)e.OldValue);
        }

        private static object CoerceNumber(DependencyObject d, object? baseValue)
        {
            if (DesignerProperties.GetIsInDesignMode(d))
            {
                return -1;
            }

            return baseValue switch
            {
                int i => i,
                _ => 0,
            };
        }

        private static bool ValidateNumber(object? value)
        {
            if (value is int)
            {
                return false;
            }

            return value switch
            {
                string s => s.Length > 1,
                _ => false,
            };
        }
    }
}
