// ReSharper disable All
namespace ValidCode
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the <see cref="Bar1"/> dependency property.</summary>
        public static readonly DependencyProperty Bar1Property = DependencyProperty.Register(
            nameof(Bar1),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        /// <summary>Identifies the <see cref="Bar2"/> dependency property.</summary>
        public static DependencyProperty Bar2Property { get; } = DependencyProperty.Register(
            "Bar2", 
            typeof(int), 
            typeof(FooControl), 
            new PropertyMetadata(default(int)));

        static FooControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FooControl), new FrameworkPropertyMetadata(typeof(FooControl)));
        }

        public int Bar1
        {
            get => (int)this.GetValue(Bar1Property);
            set => this.SetValue(Bar1Property, value);
        }

        public int Bar2
        {
            get { return (int)GetValue(Bar2Property); }
            set { SetValue(Bar2Property, value); }
        }
    }
}
