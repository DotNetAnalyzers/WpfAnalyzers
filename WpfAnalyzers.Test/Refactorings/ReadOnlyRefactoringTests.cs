namespace WpfAnalyzers.Test.Refactorings;

using Gu.Roslyn.Asserts;
using NUnit.Framework;
using WpfAnalyzers.Refactorings;

public static class ReadOnlyRefactoringTests
{
    private static readonly ReadOnlyRefactoring Refactoring = new();

    [Test]
    public static void ChangeToReadOnly()
    {
        var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class C : Control
    {
        /// <summary>Identifies the <see cref=""Number""/> dependency property.</summary>
        public static readonly DependencyProperty ↓NumberProperty = DependencyProperty.Register(
            nameof(Number),
            typeof(int),
            typeof(C),
            new PropertyMetadata(default(int)));

        public int Number
        {
            get => (int)this.GetValue(NumberProperty);
            set => this.SetValue(NumberProperty, value);
        }
    }
}";

        var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class C : Control
    {
        private static readonly DependencyPropertyKey NumberPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Number),
            typeof(int),
            typeof(C),
            new PropertyMetadata(default(int)));

        /// <summary>Identifies the <see cref=""Number""/> dependency property.</summary>
        public static readonly DependencyProperty NumberProperty = NumberPropertyKey.DependencyProperty;

        public int Number
        {
            get => (int)this.GetValue(NumberProperty);
            set => this.SetValue(NumberPropertyKey, value);
        }
    }
}";
        RoslynAssert.Refactoring(Refactoring, before, after);
    }
}