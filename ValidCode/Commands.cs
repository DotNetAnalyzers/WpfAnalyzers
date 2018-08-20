namespace ValidCode
{
    using System.Windows.Input;

    public static class Commands
    {
        public static readonly RoutedCommand Bar1Command = new RoutedCommand(nameof(Bar1Command), typeof(Commands));
        public static readonly RoutedCommand Bar2Command = new RoutedCommand("Bar2", typeof(Commands));
        public static readonly RoutedUICommand Bar3Command = new RoutedUICommand("Some text", nameof(Bar3Command), typeof(Commands));
        public static readonly RoutedUICommand Bar4Command = new RoutedUICommand("Some text", "Bar4", typeof(Commands));
    }
}
