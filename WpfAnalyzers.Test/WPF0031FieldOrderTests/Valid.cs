﻿namespace WpfAnalyzers.Test.WPF0031FieldOrderTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class Valid
{
    private static readonly DependencyPropertyBackingFieldOrPropertyAnalyzer Analyzer = new();

    [Test]
    public static void DependencyPropertyRegisterReadOnly()
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

        /// <summary>Identifies the <see cref=""Bar""/> dependency property.</summary>
        public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

        public int Bar
        {
            get { return (int)this.GetValue(BarProperty); }
            protected set {  this.SetValue(BarPropertyKey, value); }
        }
    }
}";

        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void DependencyPropertyRegisterAttachedReadOnly()
    {
        var code = @"
namespace N
{
    using System.Windows;

    public static class Foo
    {
        private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
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
        internal static readonly DependencyPropertyKey SelectedLinkPropertyKey = DependencyProperty.RegisterReadOnly(
            ""SelectedLink"",
            typeof(Link),
            typeof(Links),
            new FrameworkPropertyMetadata(null));

        /// <summary>Identifies the <see cref=""SelectedLink""/> dependency property.</summary>
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
        /// <summary>Identifies the <see cref=""SelectedLink""/> dependency property.</summary>
        public static readonly DependencyProperty SelectedLinkProperty = Links.SelectedLinkProperty.AddOwner(typeof(LinkGroup));

        public Link SelectedLink
        {
            get { return (Link)this.GetValue(SelectedLinkProperty); }
            protected set { this.SetValue(Links.SelectedLinkPropertyKey, value); }
        }
    }
}";
        RoslynAssert.Valid(Analyzer, link, links, linkGroup);
    }
}
