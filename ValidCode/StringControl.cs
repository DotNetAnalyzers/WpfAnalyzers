namespace ValidCode;

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

public class StringControl : Control
{
    /// <summary>Identifies the <see cref="Text"/> dependency property.</summary>
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text),
        typeof(string),
        typeof(StringControl),
        new PropertyMetadata(
            string.Empty,
            OnTextChanged,
            CoerceText),
        ValidateText);

    static StringControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StringControl), new FrameworkPropertyMetadata(typeof(StringControl)));
    }

    public string Text
    {
        get => (string)this.GetValue(TextProperty);
        set => this.SetValue(TextProperty, value);
    }

    protected void OnTextChanged(string oldValue, string newValue)
    {
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (DesignerProperties.GetIsInDesignMode(d))
        {
            return;
        }

        ((StringControl)d).OnTextChanged((string)e.NewValue, (string)e.OldValue);
    }

    private static object CoerceText(DependencyObject d, object? baseValue)
    {
        if (DesignerProperties.GetIsInDesignMode(d))
        {
            return string.Empty;
        }

        return baseValue switch
        {
            string s => s.Length > 1,
            _ => string.Empty,
        };
    }

    private static bool ValidateText(object? value)
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
