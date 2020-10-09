namespace WpfAnalyzers.Test.Refactorings
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeRefactorings;
    using NUnit.Framework;
    using WpfAnalyzers.Refactorings;

    public static class AutoPropertyRefactoringTests
    {
        private static readonly CodeRefactoringProvider Refactoring = new AutoPropertyRefactoring();

        [Test]
        public static void AutoProperty()
        {
            var before = @"
namespace N
{
    using System.Windows.Controls;

    public class C : Control
    {
        public int ↓Number { get; set; }
    }
}";

            var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class C : Control
    {
        public static readonly DependencyProperty NumberProperty = DependencyProperty.Register(
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
            RoslynAssert.Refactoring(Refactoring, before, after);
        }
    }
}
