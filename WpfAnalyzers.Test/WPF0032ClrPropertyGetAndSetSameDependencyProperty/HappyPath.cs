namespace WpfAnalyzers.Test.DependencyProperties.WPF0032ClrPropertyGetAndSetSameDependencyProperty
{
    using System.Threading.Tasks;

    using NUnit.Framework;

    using WpfAnalyzers.DependencyProperties;

    internal class HappyPath : HappyPathVerifier<WPF0032ClrPropertyGetAndSetSameDependencyProperty>
    {
        [Test]
        public async Task DependencyProperty()
        {
            var testCode = @"
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
    }";

            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task DependencyPropertyWithThis()
        {
            var testCode = @"
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
    }";

            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task ReadOnlyDependencyProperty()
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
        protected set { this.SetValue(BarPropertyKey, value); }
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