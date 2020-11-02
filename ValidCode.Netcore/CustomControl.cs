namespace ValidCode.Netcore
{
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;

    public class CustomControl : Control
    {
        /// <summary>Identifies the <see cref="Text"/> dependency property.</summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(CustomControl),
            new PropertyMetadata(
                null,
                OnTextChanged,
                CoerceText),
            ValidateText);

        static CustomControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomControl), new FrameworkPropertyMetadata(typeof(CustomControl)));
        }

        public string? Text
        {
            get => (string?)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }

        protected void OnTextChanged(string? oldValue, string? newValue)
        {
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(d))
            {
                return;
            }

            ((CustomControl)d).OnTextChanged((string?)e.NewValue, (string?)e.OldValue);
        }

        private static object? CoerceText(DependencyObject d, object? baseValue)
        {
            if (DesignerProperties.GetIsInDesignMode(d))
            {
                return null;
            }

            return baseValue switch
            {
                string s => s.Length > 1,
                _ => false,
            };
        }

        private static bool ValidateText(object? value)
        {
            if (value is null)
            {
                return true;
            }

            return value switch
            {
                string s => s.Length > 1,
                _ => false,
            };
        }
    }
}
