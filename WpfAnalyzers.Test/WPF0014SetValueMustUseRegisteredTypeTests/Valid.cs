namespace WpfAnalyzers.Test.WPF0014SetValueMustUseRegisteredTypeTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly SetValueAnalyzer Analyzer = new SetValueAnalyzer();

        [TestCase("this.SetValue(BarProperty, 1);")]
        [TestCase("this.SetCurrentValue(BarProperty, 1);")]
        public static void DependencyProperty(string setValueCall)
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

        public void Meh()
        {
            this.SetValue(BarProperty, 1);
        }
    }
}".AssertReplace("this.SetValue(BarProperty, 1);", setValueCall);

            RoslynAssert.Valid(Analyzer, code);
        }

        [TestCase("this.SetValue(BarProperty, 1);")]
        [TestCase("this.SetCurrentValue(BarProperty, 1);")]
        public static void DependencyPropertyPartial(string setValueCall)
        {
            var part1 = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public partial class FooControl : Control
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

            var part2 = @"
namespace N
{
    public partial class FooControl
    {
        public void Meh()
        {
            this.SetValue(BarProperty, 1);
        }
    }
}".AssertReplace("this.SetValue(BarProperty, 1);", setValueCall);
            RoslynAssert.Valid(Analyzer, part1, part2);
        }

        [TestCase("this.SetValue(BarProperty, 1);")]
        [TestCase("this.SetValue(BarProperty, null);")]
        [TestCase("this.SetCurrentValue(BarProperty, 1);")]
        [TestCase("this.SetCurrentValue(BarProperty, null);")]
        public static void DependencyPropertyOfTypeNullableInt(string setValueCall)
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
            typeof(int?),
            typeof(FooControl),
            new PropertyMetadata(default(int?)));

        public int? Bar
        {
            get { return (int?)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }

        public void Meh()
        {
            this.SetValue(BarProperty, 1);
        }
    }
}".AssertReplace("this.SetValue(BarProperty, 1);", setValueCall);

            RoslynAssert.Valid(Analyzer, code);
        }

        [TestCase("fooControl.SetValue(FooControl.BarProperty, 1);")]
        [TestCase("fooControl.SetValue(FooControl.BarProperty, null);")]
        [TestCase("fooControl.SetCurrentValue(FooControl.BarProperty, 1);")]
        [TestCase("fooControl.SetCurrentValue(FooControl.BarProperty, null);")]
        public static void DependencyPropertyOfTypeNullableFromOutside(string setValueCall)
        {
            var fooControlCode = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            ""Bar"",
            typeof(int?),
            typeof(FooControl),
            new PropertyMetadata(default(int?)));

        public int? Bar
        {
            get { return (int?)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }
    }
}";

            var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class Foo
    {
        public void Meh()
        {
            var fooControl = new FooControl();
            fooControl.SetValue(BarProperty, 1);
        }
    }
}".AssertReplace("fooControl.SetValue(BarProperty, 1);", setValueCall);

            RoslynAssert.Valid(Analyzer, fooControlCode, code);
        }

        [TestCase("this.SetValue(BarProperty, meh);")]
        [TestCase("this.SetCurrentValue(BarProperty, meh);")]
        public static void DependencyPropertyOfTypeNullableIntParameter(string setValueCall)
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
            typeof(int?),
            typeof(FooControl),
            new PropertyMetadata(default(int?)));

        public int? Bar
        {
            get { return (int?)this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
        }

        public void Meh(int meh)
        {
            this.SetValue(BarProperty, meh);
        }
    }
}".AssertReplace("this.SetValue(BarProperty, meh);", setValueCall);

            RoslynAssert.Valid(Analyzer, code);
        }

        [TestCase("this.SetValue(ValueProperty, meh);")]
        [TestCase("this.SetCurrentValue(ValueProperty, meh);")]
        public static void DependencyPropertyOfTypeNullableTParameter(string setValueCall)
        {
            var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl<T> : Control
        where T : struct
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(T?),
            typeof(FooControl<T>),
            new PropertyMetadata(default(T?)));

        public FooControl(T meh)
        {
            this.SetValue(ValueProperty, meh);
        }

        public T? Value
        {
            get => (T?)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }
    }
}".AssertReplace("this.SetValue(ValueProperty, meh);", setValueCall);

            RoslynAssert.Valid(Analyzer, code);
        }

        [TestCase("this.SetValue(BarProperty, 1);")]
        [TestCase("this.SetValue(BarProperty, null);")]
        [TestCase("this.SetCurrentValue(BarProperty, 1);")]
        [TestCase("this.SetCurrentValue(BarProperty, null);")]
        public static void DependencyPropertyOfTypeObject(string setValueCall)
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
            typeof(object),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public object Bar
        {
            get { return (object)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }

        public void Meh()
        {
            this.SetValue(BarProperty, 1);
        }
    }
}".AssertReplace("this.SetValue(BarProperty, 1);", setValueCall);

            RoslynAssert.Valid(Analyzer, code);
        }

        [TestCase("this.SetValue(BarProperty, new Foo());")]
        [TestCase("this.SetCurrentValue(BarProperty, new Foo());")]
        public static void DependencyPropertyOfInterfaceType(string setValueCall)
        {
            var interfaceCode = @"
namespace N
{
    public interface IFoo
    {
    }
}";

            var fooCode = @"
namespace N
{
    public class Foo : IFoo
    {
    }
}";
            var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            ""Bar"",
            typeof(IFoo),
            typeof(FooControl),
            new PropertyMetadata(default(IFoo)));

        public IFoo Bar
        {
            get { return (IFoo)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }

        public void Meh()
        {
            this.SetValue(BarProperty, new Foo());
        }
    }
}".AssertReplace("this.SetValue(BarProperty, new Foo());", setValueCall);

            RoslynAssert.Valid(Analyzer, interfaceCode, fooCode, code);
        }

        [TestCase("this.SetValue(BarProperty, 1);")]
        [TestCase("this.SetCurrentValue(BarProperty, 1);")]
        public static void DependencyPropertyGeneric(string setValueCall)
        {
            var fooControlGeneric = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl<T> : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            ""Bar"",
            typeof(T),
            typeof(FooControl),
            new PropertyMetadata(default(T)));

        public T Bar
        {
            get { return (T)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }
    }
}";

            var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : FooControl<int>
    {
        public void Meh()
        {
            this.SetValue(BarProperty, 1);
        }
    }
}".AssertReplace("this.SetValue(BarProperty, 1);", setValueCall);

            RoslynAssert.Valid(Analyzer, fooControlGeneric, code);
        }

        [TestCase("this.SetValue(BarProperty, (object)1);")]
        [TestCase("this.SetCurrentValue(BarProperty, (object)1);")]
        public static void DependencyPropertySetValueOfTypeObject(string setValueCall)
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

        public void Meh()
        {
            this.SetValue(BarProperty, (object)1);
        }
    }
}".AssertReplace("this.SetValue(BarProperty, (object)1);", setValueCall);

            RoslynAssert.Valid(Analyzer, code);
        }

        [TestCase("this.SetValue(BarProperty, value);")]
        [TestCase("this.SetCurrentValue(BarProperty, value);")]
        public static void DependencyPropertySetValueOfTypeObject2(string setValueCall)
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

        public void Meh()
        {
            var value = this.GetValue(BarProperty);
            this.SetValue(BarProperty, value);
        }
    }
}".AssertReplace("this.SetValue(BarProperty, value);", setValueCall);

            RoslynAssert.Valid(Analyzer, code);
        }

        [TestCase("this.SetValue(BarProperty, true);")]
        [TestCase("this.SetCurrentValue(BarProperty, true);")]
        public static void DependencyPropertyAddOwner(string setValueCall)
        {
            var fooCode = @"
namespace N
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(bool),
            typeof(Foo),
            new PropertyMetadata(default(bool)));

        public static void SetBar(FrameworkElement element, bool value)
        {
            element.SetValue(BarProperty, value);
        }

        public static bool GetBar(FrameworkElement element)
        {
            return (bool)element.GetValue(BarProperty);
        }
    }
}";

            var fooControlPart1 = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public partial class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = Foo.BarProperty.AddOwner(
            typeof(FooControl),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnVolumeChanged,
                OnVolumeCoerce));

        public bool Bar
        {
            get { return (bool)this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
        }

        private static object OnVolumeCoerce(DependencyObject d, object baseValue)
        {
            return baseValue;
        }

        private static void OnVolumeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // nop
        }
    }
}";

            var fooControlPart2 = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public partial class FooControl
    {
        public FooControl()
        {
            this.SetValue(BarProperty, false);
        }
    }
}".AssertReplace("this.SetValue(BarProperty, false);", setValueCall);
            RoslynAssert.Valid(Analyzer, fooCode, fooControlPart1, fooControlPart2);
        }

        [TestCase("this.SetValue(VolumeProperty, 1.0);")]
        [TestCase("this.SetCurrentValue(VolumeProperty, 1.0);")]
        public static void DependencyPropertyAddOwnerMediaElementVolume(string setValueCall)
        {
            var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class MediaElementWrapper : Control
    {
        public static readonly DependencyProperty VolumeProperty = MediaElement.VolumeProperty.AddOwner(
            typeof(MediaElementWrapper),
            new FrameworkPropertyMetadata(
                MediaElement.VolumeProperty.DefaultMetadata.DefaultValue,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnVolumeChanged,
                OnVolumeCoerce));

        public MediaElementWrapper()
        {
            this.SetValue(VolumeProperty, 2.0);
        }

        public double Volume
        {
            get { return (double)this.GetValue(VolumeProperty); }
            set { this.SetValue(VolumeProperty, value); }
        }

        private static object OnVolumeCoerce(DependencyObject d, object baseValue)
        {
            return baseValue;
        }

        private static void OnVolumeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // nop
        }
    }
}".AssertReplace("this.SetValue(VolumeProperty, 2.0);", setValueCall);

            RoslynAssert.Valid(Analyzer, code);
        }

        [TestCase("textBox.SetValue(TextBox.TextProperty, \"abc\");")]
        [TestCase("textBox.SetCurrentValue(TextBox.TextProperty, \"abc\");")]
        public static void TextBoxText(string setValueCall)
        {
            var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public static class Foo
    {
        public static void Bar()
        {
            var textBox = new TextBox();
            textBox.SetValue(TextBox.TextProperty, ""abc"");
        }
    }
}".AssertReplace("textBox.SetValue(TextBox.TextProperty, \"abc\");", setValueCall);

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void SetCurrentValueInLambda()
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

        public void Meh()
        {
            this.Loaded += (sender, args) =>
            {
                this.SetCurrentValue(BarProperty, 1);
            };
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [TestCase("SetValue")]
        [TestCase("SetCurrentValue")]
        public static void IgnoredPropertyAsParameter(string setValueCall)
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

        public int Bar
        {
            get { return (int)this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
        }

        public void Meh(DependencyProperty property, object value)
        {
            this.SetCurrentValue(property, value);
        }
    }
}".AssertReplace("SetCurrentValue", setValueCall);

            RoslynAssert.Valid(Analyzer, code);
        }

        [TestCase("SetValue")]
        [TestCase("SetCurrentValue")]
        public static void IgnoresFreezable(string call)
        {
            var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BrushProperty = DependencyProperty.Register(
            nameof(Brush),
            typeof(Brush),
            typeof(FooControl),
            new PropertyMetadata(default(Brush)));

        public Brush Brush
        {
            get { return (Brush)this.GetValue(BrushProperty); }
            set { this.SetValue(BrushProperty, value); }
        }

        public void UpdateBrush(Brush brush)
        {
            this.SetCurrentValue(BrushProperty, brush?.GetAsFrozen());
        }
    }
}".AssertReplace("SetCurrentValue", call);

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void ObservableCollectionOfIntWhenIEnumerable()
        {
            var code = @"
namespace N
{
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            nameof(Source),
            typeof(IEnumerable),
            typeof(FooControl),
            new PropertyMetadata(default(IEnumerable)));

        public IEnumerable Source
        {
            get => (IEnumerable)this.GetValue(SourceProperty);
            set => this.SetValue(SourceProperty, value);
        }

        public static void M()
        {
            var control = new FooControl();
            control.SetValue(SourceProperty, new ObservableCollection<int> { 1, 2 });
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void ObservableCollectionOfIntWhenNullableIEnumerable()
        {
            var code = @"
#nullable enable
namespace N
{
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            nameof(Source),
            typeof(IEnumerable),
            typeof(FooControl),
            new PropertyMetadata(default(IEnumerable)));

        public IEnumerable? Source
        {
            get => (IEnumerable)this.GetValue(SourceProperty);
            set => this.SetValue(SourceProperty, value);
        }

        public static void M()
        {
            var control = new FooControl();
            control.SetValue(SourceProperty, new ObservableCollection<int> { 1, 2 });
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void ObservableCollectionOfIntWhenIEnumerableAttached()
        {
            var code = @"
namespace N
{
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Controls;

    public static class C
    {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.RegisterAttached(
            ""Source"",
            typeof(IEnumerable),
            typeof(C),
            new PropertyMetadata(default(IEnumerable)));

        public static void SetSource(DependencyObject element, IEnumerable value)
        {
            element.SetValue(SourceProperty, value);
        }

        public static IEnumerable GetSource(DependencyObject element)
        {
            return (IEnumerable)element.GetValue(SourceProperty);
        }

        public static void M()
        {
            var control = new DataGrid();
            control.SetValue(SourceProperty, new ObservableCollection<int> { 1, 2 });
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void ObservableCollectionOfIntWhenIEnumerableAttachedOtherProject()
        {
            var c1 = @"
namespace N1
{
    using System.Collections;
    using System.Windows;

    public static class C1
    {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.RegisterAttached(
            ""Source"",
            typeof(IEnumerable),
            typeof(C1),
            new PropertyMetadata(default(IEnumerable)));

        public static void SetSource(DependencyObject element, IEnumerable value)
        {
            element.SetValue(SourceProperty, value);
        }

        public static IEnumerable GetSource(DependencyObject element)
        {
            return (IEnumerable)element.GetValue(SourceProperty);
        }
    }
}";

            var code = @"
namespace N2
{
    using System.Collections.ObjectModel;
    using System.Windows.Controls;
    using N1;

    class C2
    {
        public static void M()
        {
            var control = new DataGrid();
            control.SetValue(C1.SourceProperty, new ObservableCollection<int> { 1, 2 });
        }
    }
}";
            RoslynAssert.Valid(Analyzer, c1, code);
        }

        [Test]
        public static void PropertyKeyInOtherClass()
        {
            var linkCode = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;

    public class Link : ButtonBase
    {
    }
}";

            var modernLinksCode = @"
namespace N
{
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
    }
}";

            var linkGroupCode = @"
namespace N
{
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
    }
}";
            RoslynAssert.Valid(Analyzer, linkCode, modernLinksCode, linkGroupCode);
        }

        [Test]
        public static void CastIntToDouble()
        {
            var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public sealed class FooControl : Control
    {
        private const int DefaultValue = 1;

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(FooControl),
            new PropertyMetadata(default(double)));

        public FooControl()
        {
            this.SetValue(ValueProperty, (double)DefaultValue);
        }

        public double Value
        {
            get => (double)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }
    }
}";
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

        [Test]
        public static void Issue278NullCoalesce()
        {
            var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Media;

    public class Issue278 : FrameworkElement
    {
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
            nameof(Color),
            typeof(Color),
            typeof(Issue278),
            new PropertyMetadata(default(Color)));

        public Color Color
        {
            get => (Color)this.GetValue(ColorProperty);
            set => this.SetValue(ColorProperty, value);
        }

        public void M(Issue278 control, Color? color = null)
        {
            control.SetCurrentValue(ColorProperty, color ?? Colors.HotPink);
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void Issue278NewList()
        {
            var code = @"
namespace N
{
     using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Media;

    public class Issue278 : FrameworkElement
    {
        /// <summary>Identifies the <see cref=""Colors""/> dependency property.</summary>
        public static readonly DependencyProperty ColorsProperty = DependencyProperty.Register(
            nameof(Colors),
            typeof(List<Color>),
            typeof(Issue278),
            new PropertyMetadata(default(List<Color>)));

        public List<Color> Colors
        {
            get => (List<Color>)this.GetValue(ColorsProperty);
            set => this.SetValue(ColorsProperty, value);
        }

        public void M(Issue278 control, Color? color = null)
        {
            control.SetCurrentValue(ColorsProperty, new List<Color> { System.Windows.Media.Colors.HotPink });
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }
    }
}
