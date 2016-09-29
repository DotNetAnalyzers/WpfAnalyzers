namespace WpfAnalyzers.Demo
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            nameof(Bar),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        private static readonly DependencyPropertyKey BarReadOnlyPropertyKey = DependencyProperty.RegisterReadOnly(
            "BarReadOnly",
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public static readonly DependencyProperty BarReadOnlyProperty = BarReadOnlyPropertyKey.DependencyProperty;

        public int Bar
        {
            get { return (int)this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
        }

        public int BarReadOnly
        {
            get { return (int)this.GetValue(BarReadOnlyProperty); }
            protected set { this.SetValue(BarReadOnlyPropertyKey, value); }
        }
    }
}
