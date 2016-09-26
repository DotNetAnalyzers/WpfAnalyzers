namespace WpfAnalyzers.Demo
{
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            "Bar",
            typeof(ObservableCollection<int>),
            typeof(FooControl),
            new PropertyMetadata(new ObservableCollection<int>()));

        public ObservableCollection<int> Bar
        {
            get { return (ObservableCollection<int>)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }
    }
}
