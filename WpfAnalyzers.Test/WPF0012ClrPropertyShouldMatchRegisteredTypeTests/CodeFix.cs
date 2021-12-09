namespace WpfAnalyzers.Test.WPF0012ClrPropertyShouldMatchRegisteredTypeTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly ClrPropertyDeclarationAnalyzer Analyzer = new();
        private static readonly UseRegisteredTypeFix Fix = new();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0012ClrPropertyShouldMatchRegisteredType);

        [Test]
        public static void Message()
        {
            var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            nameof(Bar), 
            typeof(int), 
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public ↓double Bar
        {
            get => (double)this.GetValue(BarProperty);
            set => this.SetValue(BarProperty, value);
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("Property 'N.FooControl.Bar' must be of type int"), code);
        }

        [TestCase("double")]
        [TestCase("int?")]
        [TestCase("Nullable<int>")]
        [TestCase("ObservableCollection<int>")]
        public static void DependencyProperty(string typeName)
        {
            var before = @"
namespace N
{
    using System;
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            nameof(Bar),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public ↓double Bar
        {
            get { return (double)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }
    }
}".AssertReplace("double", typeName);

            var after = @"
namespace N
{
    using System;
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            nameof(Bar),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

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
        public static void DependencyPropertyWithThis()
        {
            var before = @"
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

        public ↓double Bar
        {
            get { return (double)this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
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

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void DependencyPropertyExpressionBodyAccessors()
        {
            var before = @"
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

        public ↓double Bar
        {
            get => (double)this.GetValue(BarProperty);
            set => this.SetValue(BarProperty, value);
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
            ""Bar"", 
            typeof(int), 
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public int Bar
        {
            get => (int)this.GetValue(BarProperty);
            set => this.SetValue(BarProperty, value);
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void AddOwnerAttachedPropertyInSource()
        {
            var foo = @"
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
        public static readonly DependencyProperty BarProperty = Foo.BarProperty.AddOwner(typeof(FooControl));

        public ↓double Bar
        {
            get { return (double) this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
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

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { foo, before }, after);
        }

        [Test]
        public static void AddOwnerTextElementFontSize()
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Documents;

    public class FooControl : FrameworkElement
    {
        public static readonly DependencyProperty FontSizeProperty = TextElement.FontSizeProperty.AddOwner(typeof(FooControl));

        public ↓int FontSize
        {
            get => (int)this.GetValue(FontSizeProperty);
            set => this.SetValue(FontSizeProperty, value);
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
        public static readonly DependencyProperty BorderThicknessProperty = Border.BorderThicknessProperty.AddOwner(typeof(FooControl));

        public ↓int BorderThickness
        {
            get => (int)GetValue(BorderThicknessProperty);
            set => SetValue(BorderThicknessProperty, value);
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

        public Thickness BorderThickness
        {
            get => (Thickness)GetValue(BorderThicknessProperty);
            set => SetValue(BorderThicknessProperty, value);
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void ReadOnlyDependencyProperty()
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

        public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

        public ↓double Bar
        {
            get { return (double)this.GetValue(BarProperty); }
            protected set { this.SetValue(BarPropertyKey, value); }
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
            get { return (int)this.GetValue(BarProperty); }
            protected set { this.SetValue(BarPropertyKey, value); }
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void DependencyPropertyNullable()
        {
            var before = @"
namespace BoxEr.Ui.Views.Status
{
    using System.Windows;
    using System.Windows.Controls;

    public class C : Control
    {
        /// <summary>Identifies the <see cref=""Number""/> dependency property.</summary>
        public static readonly DependencyProperty NumberProperty = DependencyProperty.Register(
            nameof(Number),
            typeof(int?),
            typeof(C),
            new PropertyMetadata(default(int?)));

        public ↓int Number
        {
            get => (int)GetValue(NumberProperty);
            set => SetValue(NumberProperty, value);
        }
    }
}";

            var after = @"
namespace BoxEr.Ui.Views.Status
{
    using System.Windows;
    using System.Windows.Controls;

    public class C : Control
    {
        /// <summary>Identifies the <see cref=""Number""/> dependency property.</summary>
        public static readonly DependencyProperty NumberProperty = DependencyProperty.Register(
            nameof(Number),
            typeof(int?),
            typeof(C),
            new PropertyMetadata(default(int?)));

        public int? Number
        {
            get => (int?)GetValue(NumberProperty);
            set => SetValue(NumberProperty, value);
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void DependencyPropertyNullableNotCastingReturnValueCorrectly()
        {
            var before = @"
namespace BoxEr.Ui.Views.Status
{
    using System.Windows;
    using System.Windows.Controls;

    public class C : Control
    {
        /// <summary>Identifies the <see cref=""Number""/> dependency property.</summary>
        public static readonly DependencyProperty NumberProperty = DependencyProperty.Register(
            nameof(Number),
            typeof(int?),
            typeof(C),
            new PropertyMetadata(default(int?)));

        public int? Number
        {
            get => (↓int)GetValue(NumberProperty);
            set => SetValue(NumberProperty, value);
        }
    }
}";

            var after = @"
namespace BoxEr.Ui.Views.Status
{
    using System.Windows;
    using System.Windows.Controls;

    public class C : Control
    {
        /// <summary>Identifies the <see cref=""Number""/> dependency property.</summary>
        public static readonly DependencyProperty NumberProperty = DependencyProperty.Register(
            nameof(Number),
            typeof(int?),
            typeof(C),
            new PropertyMetadata(default(int?)));

        public int? Number
        {
            get => (int?)GetValue(NumberProperty);
            set => SetValue(NumberProperty, value);
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
