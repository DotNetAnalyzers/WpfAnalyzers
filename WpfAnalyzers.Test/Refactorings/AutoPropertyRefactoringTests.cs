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
        /// <summary>Identifies the <see cref=""Number""/> dependency property.</summary>
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

        [Test]
        public static void AutoProperty2()
        {
            var before = @"
namespace N
{
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public double? ↓Value { get; set; }
    }
}";

            var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the <see cref=""Value""/> dependency property.</summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double?),
            typeof(FooControl),
            new PropertyMetadata(default(double?)));

        public double? Value
        {
            get => (double?)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }
    }
}";
            RoslynAssert.Refactoring(Refactoring, before, after);
        }

        [Test]
        public static void AutoPropertyNotQualifiedMethodAccess()
        {
            var before = @"
namespace N
{
    using System.Windows.Controls;

    public class C : Control
    {
        public int ↓Number { get; set; }

        public int M() => M();
    }
}";

            var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class C : Control
    {
        /// <summary>Identifies the <see cref=""Number""/> dependency property.</summary>
        public static readonly DependencyProperty NumberProperty = DependencyProperty.Register(
            nameof(Number),
            typeof(int),
            typeof(C),
            new PropertyMetadata(default(int)));

        public int Number
        {
            get => (int)GetValue(NumberProperty);
            set => SetValue(NumberProperty, value);
        }

        public int M() => M();
    }
}";
            RoslynAssert.Refactoring(Refactoring, before, after);
        }
    }
}
