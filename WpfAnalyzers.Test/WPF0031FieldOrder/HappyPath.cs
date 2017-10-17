namespace WpfAnalyzers.Test.WPF0031FieldOrder
{
    using System.Threading.Tasks;
    using NUnit.Framework;
    using WPF0031FieldOrder = WpfAnalyzers.WPF0031FieldOrder;

    internal class HappyPath : HappyPathVerifier<WPF0031FieldOrder>
    {
        [Test]
        public async Task DependencyProperty()
        {
            var testCode = @"
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
        protected set {  this.SetValue(BarPropertyKey, value); }
    }
}";

            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task Attached()
        {
            var testCode = @"
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
}";

            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task PropertyKeyInOtherClass()
        {
            var linkCode = @"
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

public class Link : ButtonBase
{
}";

            var modernLinksCode = @"
using System.Windows;
using System.Windows.Controls;

public class ModernLinks : ItemsControl
{
    /// <summary>
    /// Identifies the SelectedSource dependency property.
    /// </summary>
    internal static readonly DependencyPropertyKey SelectedLinkPropertyKey = DependencyProperty.RegisterReadOnly(
        ""SelectedLink"",
        typeof(Link),
        typeof(ModernLinks),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty SelectedLinkProperty = SelectedLinkPropertyKey.DependencyProperty;
}";

            var linkGroupCode = @"
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

public class LinkGroup : ButtonBase
{
    public static readonly DependencyProperty SelectedLinkProperty = ModernLinks.SelectedLinkProperty.AddOwner(typeof(LinkGroup));

    public Link SelectedLink
    {
        get { return (Link)this.GetValue(SelectedLinkProperty); }
        protected set { this.SetValue(ModernLinks.SelectedLinkPropertyKey, value); }
    }
}";
            await this.VerifyHappyPathAsync(new[] { linkCode, modernLinksCode, linkGroupCode }).ConfigureAwait(false);
        }
    }
}