namespace WpfAnalyzers.Test.WPF0032ClrPropertyGetAndSetSameDependencyPropertyTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class Valid
{
    private static readonly ClrPropertyDeclarationAnalyzer Analyzer = new();

    [Test]
    public static void DependencyProperty()
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
            get { return (int)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }
    }
}";

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
    public static void ReadOnlyDependencyProperty()
    {
        var code = @"
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

        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void ReadOnlyDependencyPropertyWithNonStandardKeyField()
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        private static readonly DependencyPropertyKey BarKey = DependencyProperty.RegisterReadOnly(
            ""Bar"",
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public static readonly DependencyProperty BarProperty = BarKey.DependencyProperty;

        public int Bar
        {
            get { return (int)this.GetValue(BarProperty); }
            protected set { this.SetValue(BarKey, value); }
        }
    }
}";

        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void PropertyKeyInOtherClass()
    {
        var link = @"
namespace N
{
    using System.Windows.Controls.Primitives;

    public class Link : ButtonBase
    {
    }
}";

        var links = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class Links : ItemsControl
    {
        /// <summary>
        /// Identifies the SelectedSource dependency property.
        /// </summary>
        internal static readonly DependencyPropertyKey SelectedLinkPropertyKey = DependencyProperty.RegisterReadOnly(
            ""SelectedLink"",
            typeof(Link),
            typeof(Links),
            new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty SelectedLinkProperty = SelectedLinkPropertyKey.DependencyProperty;
    }
}";

        var linkGroup = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls.Primitives;

    public class LinkGroup : ButtonBase
    {
        public static readonly DependencyProperty SelectedLinkProperty = Links.SelectedLinkProperty.AddOwner(typeof(LinkGroup));

        public Link? SelectedLink
        {
            get { return (Link?)this.GetValue(SelectedLinkProperty); }
            protected set { this.SetValue(Links.SelectedLinkPropertyKey, value); }
        }
    }
}";
        RoslynAssert.Valid(Analyzer, link, links, linkGroup);
    }
}