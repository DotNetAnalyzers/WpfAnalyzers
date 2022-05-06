namespace ValidCode.Issues;

using System.Windows;

public class Issue376 : FrameworkElement
{
    private static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text),
        typeof(string),
        typeof(Issue376),
        new PropertyMetadata(default(string)));

    private string? Text
    {
        get => (string?)this.GetValue(TextProperty);
        set => this.SetValue(TextProperty, value);
    }

    public void M(string text)
    {
        this.Text = text;
    }
}
