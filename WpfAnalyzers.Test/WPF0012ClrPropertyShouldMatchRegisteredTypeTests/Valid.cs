namespace WpfAnalyzers.Test.WPF0012ClrPropertyShouldMatchRegisteredTypeTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly ClrPropertyDeclarationAnalyzer Analyzer = new ClrPropertyDeclarationAnalyzer();

        [TestCase("int")]
        [TestCase("int?")]
        [TestCase("Nullable<int>")]
        [TestCase("int[]")]
        [TestCase("int?[]")]
        [TestCase("ObservableCollection<int>")]
        public static void DependencyProperty(string typeName)
        {
            var code = @"
namespace N
{
    using System;
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            ""Bar"", 
            typeof(int), 
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }
    }
}".AssertReplace("int", typeName);

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void DependencyPropertyWithThis()
        {
            var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            ""Bar"", 
            typeof(int), 
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int)this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void DependencyPropertyGeneric()
        {
            var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl<T> : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            ""Bar"", 
            typeof(T), 
            typeof(FooControl<T>),
            new PropertyMetadata(default(T)));

        public T Bar
        {
            get { return (T)this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void DependencyPropertyAddOwner()
        {
            var part1 = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = Foo.BarProperty.AddOwner(typeof(FooControl));

        public int Bar
        {
            get { return (int) this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
        }
    }
}";

            var part2 = @"
namespace N
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int), 
            typeof(Foo), 
            new FrameworkPropertyMetadata(
                default(int), 
                FrameworkPropertyMetadataOptions.Inherits));

        public static void SetBar(DependencyObject element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        public static int GetBar(DependencyObject element)
        {
            return (int) element.GetValue(BarProperty);
        }
    }
}";

            RoslynAssert.Valid(Analyzer, part1, part2);
        }

        [TestCase("int")]
        [TestCase("int?")]
        [TestCase("Nullable<int>")]
        [TestCase("int[]")]
        [TestCase("int?[]")]
        [TestCase("ObservableCollection<int>")]
        public static void ReadOnlyDependencyProperty(string typeName)
        {
            var code = @"
namespace N
{
    using System;
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterReadOnly(
            ""Bar"",
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

        public int Bar
        {
            get { return (int)this.GetValue(BarProperty); }
            protected set { this.SetValue(BarPropertyKey, value); }
        }
    }
}".AssertReplace("int", typeName);

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void EnumIssue211()
        {
            var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the <see cref=""FooEnum""/> dependency property.</summary>
        public static readonly DependencyProperty FooEnumProperty = DependencyProperty.Register(
            nameof(FooEnum),
            typeof(FooEnum),
            typeof(FooControl),
            new PropertyMetadata(FooEnum.Bar));

        public FooEnum FooEnum
        {
            get => (FooEnum) this.GetValue(FooEnumProperty);
            set => this.SetValue(FooEnumProperty, value);
        }
    }
}";
            var enumCode = @"namespace N
{
    public enum FooEnum
    {
        Bar,
        Baz
    }
}";
            RoslynAssert.Valid(Analyzer, code, enumCode);
        }

        [Test]
        public static void EnumAddOwnerIssue211()
        {
            var fooCode = @"
namespace N
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty FooEnumProperty = DependencyProperty.RegisterAttached(
            ""FooEnum"",
            typeof(FooEnum),
            typeof(Foo),
            new FrameworkPropertyMetadata(FooEnum.Baz, FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>Helper for setting <see cref=""FooEnumProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""DependencyObject""/> to set <see cref=""FooEnumProperty""/> on.</param>
        /// <param name=""value"">FooEnum property value.</param>
        public static void SetFooEnum(DependencyObject element, FooEnum value)
        {
            element.SetValue(FooEnumProperty, value);
        }

        /// <summary>Helper for getting <see cref=""FooEnumProperty""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""DependencyObject""/> to read <see cref=""FooEnumProperty""/> from.</param>
        /// <returns>FooEnum property value.</returns>
        public static FooEnum GetFooEnum(DependencyObject element)
        {
            return (FooEnum)element.GetValue(FooEnumProperty);
        }
    }
}";
            var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the <see cref=""FooEnum""/> dependency property.</summary>
        public static readonly DependencyProperty FooEnumProperty = Foo.FooEnumProperty.AddOwner(
            typeof(FooControl),
            new FrameworkPropertyMetadata(
                FooEnum.Bar,
                FrameworkPropertyMetadataOptions.Inherits));

        public FooEnum FooEnum
        {
            get => (FooEnum) this.GetValue(FooEnumProperty);
            set => this.SetValue(FooEnumProperty, value);
        }
    }
}";
            var enumCode = @"namespace N
{
    public enum FooEnum
    {
        Bar,
        Baz
    }
}";
            RoslynAssert.Valid(Analyzer, fooCode, code, enumCode);
        }
    }
}
