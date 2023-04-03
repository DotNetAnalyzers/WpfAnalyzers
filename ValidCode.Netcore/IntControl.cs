namespace ValidCode.Netcore;

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

public class IntControl : Control
{
    /// <summary>Identifies the <see cref="Number"/> dependency property.</summary>
    public static readonly DependencyProperty NumberProperty = DependencyProperty.Register(
        nameof(Number),
        typeof(int),
        typeof(IntControl),
        new PropertyMetadata(
            0,
            OnNumberChanged,
            CoerceNumber),
        ValidateNumber);

    static IntControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(IntControl), new FrameworkPropertyMetadata(typeof(IntControl)));
    }

    public int Number
    {
        get => (int)this.GetValue(NumberProperty);
        set => this.SetValue(NumberProperty, value);
    }

    protected void OnNumberChanged(int oldValue, int newValue)
    {
    }

    private static void OnNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (DesignerProperties.GetIsInDesignMode(d))
        {
            return;
        }

        ((IntControl)d).OnNumberChanged((int)e.NewValue, (int)e.OldValue);
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
