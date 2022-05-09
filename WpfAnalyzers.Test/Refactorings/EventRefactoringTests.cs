namespace WpfAnalyzers.Test.Refactorings;

using Gu.Roslyn.Asserts;
using NUnit.Framework;
using WpfAnalyzers.Refactorings;

public static class EventRefactoringTests
{
    private static readonly EventRefactoring Refactoring = new();

    [Test]
    public static void Event()
    {
        var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class C : Control
    {
        public event RoutedEventHandler? ↓ValueChanged;
    }
}";

        var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class C : Control
    {
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(ValueChanged),
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(C));

        public event RoutedEventHandler? ValueChanged
        {
            add => this.AddHandler(ValueChangedEvent, value);
            remove => this.RemoveHandler(ValueChangedEvent, value);
        }
    }
}";
        RoslynAssert.Refactoring(Refactoring, before, after);
    }
}
