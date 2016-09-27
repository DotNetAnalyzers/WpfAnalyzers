namespace WpfAnalyzers.Demo
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterReadOnly(
            "Bar",
            typeof(object),
            typeof(FooControl),
            new PropertyMetadata(default(object)));

        public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

        public void Error()
        {
            this.SetValue(BarPropertyKey, null);
            this.SetCurrentValue(BarProperty, null);
        }
    }
}
