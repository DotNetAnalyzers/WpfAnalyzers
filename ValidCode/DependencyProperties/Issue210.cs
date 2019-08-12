namespace ValidCode.DependencyProperties
{
    using System.Windows;

    public class Issue210 : FrameworkElement
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            "Bar",
            typeof(string),
            typeof(Issue210),
            new FrameworkPropertyMetadata(OnBarChanged));

        public void UpdateMagic() // <-- Error WPF0005 Method 'UpdateMagic' should be named 'OnBarChanged'
        {
        }

        public void Refresh() // <-- Error WPF0005 Method 'Refresh' should be named 'OnBarChanged'
        {
        }

        static void OnBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (Issue210)d;
            control.UpdateMagic();
            control.Refresh();
        }
    }
}
