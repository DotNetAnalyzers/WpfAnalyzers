namespace WpfAnalyzers.Demo
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static System.Windows.DependencyProperty BarProperty = DependencyProperty.Register(
            nameof(Bar),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public int Bar
        {
            get
            {
                return (int)this.GetValue(BarProperty);
            }
            set
            {
                this.SetValue(BarProperty, value);
            }
        }
    }
}
