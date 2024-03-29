﻿namespace WpfAnalyzers.Test.WPF0001BackingFieldShouldMatchRegisteredNameTests;

using Gu.Roslyn.Asserts;

using NUnit.Framework;

public static class CodeFix
{
    private static readonly DependencyPropertyBackingFieldOrPropertyAnalyzer Analyzer = new();
    private static readonly RenameMemberFix Fix = new();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0001BackingFieldShouldMatchRegisteredName);

    [Test]
    public static void Message()
    {
        var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty ↓Error = DependencyProperty.Register(
            ""Bar"", typeof(int), typeof(FooControl), new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int)GetValue(Error); }
            set { SetValue(Error, value); }
        }
    }
}";

        var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            ""Bar"", typeof(int), typeof(FooControl), new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }
    }
}";
        var expectedDiagnostic = ExpectedDiagnostic.WithMessage("Field 'Error' that is backing field for the DependencyProperty registered as 'Bar' should be named 'BarProperty'");
        RoslynAssert.CodeFix(Analyzer, Fix, expectedDiagnostic, before, after, fixTitle: "Rename to: 'BarProperty'.");
    }

    [Test]
    public static void DependencyPropertyRegister()
    {
        var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty ↓Error = DependencyProperty.Register(
            ""Bar"", typeof(int), typeof(FooControl), new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int)GetValue(Error); }
            set { SetValue(Error, value); }
        }
    }
}";

        var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            ""Bar"", typeof(int), typeof(FooControl), new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void DependencyPropertyRegisterPartial()
    {
        var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public partial class FooControl
    {
        public static readonly DependencyProperty ↓Error = BarPropertyKey.DependencyProperty;

        public int Bar
        {
            get { return (int)GetValue(Error); }
            set { SetValue(BarPropertyKey, value); }
        }
    }
}";

        var part2 = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public partial class FooControl : Control
    {
        private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterReadOnly(
            ""Bar"",
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));
    }
}";

        var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public partial class FooControl
    {
        public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

        public int Bar
        {
            get { return (int)GetValue(BarProperty); }
            set { SetValue(BarPropertyKey, value); }
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { before, part2 }, after, settings: Settings.Default.WithCompilationOptions(x => x.WithSuppressedDiagnostics("CS8602")));
    }

    [Test]
    public static void DependencyPropertyRegisterReadOnly()
    {
        var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterReadOnly(
            ""Bar"",
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public static readonly DependencyProperty ↓ErrorProperty = BarPropertyKey.DependencyProperty;

        public int Bar
        {
            get { return (int)GetValue(ErrorProperty); }
            set { SetValue(BarPropertyKey, value); }
        }
    }
}";

        var after = @"
namespace N
{
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
            get { return (int)GetValue(BarProperty); }
            set { SetValue(BarPropertyKey, value); }
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void DependencyPropertyRegisterAttached()
    {
        var before = @"
namespace N
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty ↓Error = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        public static void SetBar(DependencyObject element, int value)
        {
            element.SetValue(Error, value);
        }

        public static int GetBar(DependencyObject element)
        {
            return (int)element.GetValue(Error);
        }
    }
}";

        var after = @"
namespace N
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        public static void SetBar(DependencyObject element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        public static int GetBar(DependencyObject element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void DependencyPropertyRegisterAttachedReadOnly()
    {
        var before = @"
namespace N
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        public static readonly DependencyProperty ↓Error = BarPropertyKey.DependencyProperty;

        public static void SetBar(DependencyObject element, int value)
        {
            element.SetValue(BarPropertyKey, value);
        }

        public static int GetBar(DependencyObject element)
        {
            return (int)element.GetValue(Error);
        }
    }
}";

        var after = @"
namespace N
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

        public static void SetBar(DependencyObject element, int value)
        {
            element.SetValue(BarPropertyKey, value);
        }

        public static int GetBar(DependencyObject element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void DependencyPropertyAddOwner()
    {
        var fooCode = @"
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

        var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty ↓Error = Foo.BarProperty.AddOwner(typeof(FooControl));

        public int Bar
        {
            get { return (int) this.GetValue(Error); }
            set { this.SetValue(Error, value); }
        }
    }
}";

        var after = @"
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
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { before, fooCode }, after);
    }

    [Test]
    public static void AddOwnerTextElementFontSizeProperty()
    {
        var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Documents;

    public class FooControl : FrameworkElement
    {
        public static readonly DependencyProperty ↓Error = TextElement.FontSizeProperty.AddOwner(typeof(FooControl));

        public double FontSize
        {
            get => (double)this.GetValue(Error);
            set => this.SetValue(Error, value);
        }
    }
}";

        var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Documents;

    public class FooControl : FrameworkElement
    {
        public static readonly DependencyProperty FontSizeProperty = TextElement.FontSizeProperty.AddOwner(typeof(FooControl));

        public double FontSize
        {
            get => (double)this.GetValue(FontSizeProperty);
            set => this.SetValue(FontSizeProperty, value);
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void AddOwnerBorderBorderThicknessProperty()
    {
        var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : FrameworkElement
    {
        public static readonly DependencyProperty ↓Error = Border.BorderThicknessProperty.AddOwner(typeof(FooControl));

        public Size BorderThickness
        {
            get => (Size)GetValue(Error);
            set => SetValue(Error, value);
        }
    }
}";

        var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : FrameworkElement
    {
        public static readonly DependencyProperty BorderThicknessProperty = Border.BorderThicknessProperty.AddOwner(typeof(FooControl));

        public Size BorderThickness
        {
            get => (Size)GetValue(BorderThicknessProperty);
            set => SetValue(BorderThicknessProperty, value);
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }
}
