// ReSharper disable All
namespace ValidCode
{
    using System.Windows.Input;

    public static class Commands
    {
        public static readonly RoutedCommand Command1 = new(nameof(Command1), typeof(Commands));
        public static readonly RoutedUICommand Command2 = new("Some text", nameof(Command2), typeof(Commands));
#pragma warning disable WPF0150 // Use nameof().
        public static readonly RoutedCommand Command3 = new("Command3", typeof(Commands));
        public static readonly RoutedUICommand Command4 = new("Some text", "Command4", typeof(Commands));
#pragma warning restore WPF0150 // Use nameof().
    }
}
