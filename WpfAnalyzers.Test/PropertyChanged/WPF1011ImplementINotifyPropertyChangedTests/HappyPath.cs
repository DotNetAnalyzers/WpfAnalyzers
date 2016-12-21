namespace WpfAnalyzers.Test.PropertyChanged.WPF1011ImplementINotifyPropertyChangedTests
{
    using System.Threading.Tasks;
    using NUnit.Framework;
    using WpfAnalyzers.PropertyChanged;

    internal class HappyPath : HappyPathVerifier<WPF1011ImplementINotifyPropertyChanged>
    {
        [TestCase("null")]
        [TestCase("string.Empty")]
        [TestCase(@"""Bar""")]
        [TestCase(@"nameof(Bar)")]
        [TestCase(@"nameof(this.Bar)")]
        public async Task CallsOnPropertyChanged(string propertyName)
        {
            var testCode = @"
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (value == this.bar) return;
                this.bar = value;
                this.OnPropertyChanged(nameof(Bar));
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }";

            testCode = testCode.AssertReplace(@"nameof(Bar)", propertyName);
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [TestCase("null")]
        [TestCase("string.Empty")]
        [TestCase(@"""""")]
        [TestCase(@"""Bar""")]
        [TestCase(@"nameof(Bar)")]
        [TestCase(@"nameof(this.Bar)")]
        public async Task CallsRaisePropertyChangedWithEventArgs(string propertyName)
        {
            var testCode = @"
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (value == this.bar) return;
                this.bar = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Bar)));
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }";

            testCode = testCode.AssertReplace(@"nameof(Bar)", propertyName);
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task CallsRaisePropertyChangedCallerMemberName()
        {
            var testCode = @"
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (value == this.bar) return;
                this.bar = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }";

            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [TestCase("null")]
        [TestCase("string.Empty")]
        [TestCase(@"""""")]
        [TestCase(@"""Bar""")]
        [TestCase(@"nameof(Bar)")]
        [TestCase(@"nameof(this.Bar)")]
        public async Task Invokes(string propertyName)
        {
            var testCode = @"
    using System.ComponentModel;

    public class ViewModel : INotifyPropertyChanged
    {
        private int bar;
        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (value == this.bar)
                {
                    return;
                }

                this.bar = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Bar))));
            }
        }
    }";
            testCode = testCode.AssertReplace(@"nameof(this.Bar))", propertyName);
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task InvokesCached()
        {
            var testCode = @"
    using System.ComponentModel;

    public class ViewModel : INotifyPropertyChanged
    {
        private static readonly PropertyChangedEventArgs BarPropertyChangedArgs = new PropertyChangedEventArgs(nameof(Bar));
        private int bar;
        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (value == this.bar)
                {
                    return;
                }

                this.bar = value;
                this.PropertyChanged?.Invoke(this, BarPropertyChangedArgs);
            }
        }
    }";
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task IgnoreStruct()
        {
            var testCode = @"
public struct Foo
{
    public int Bar { get; set; }
}";

            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task IgnoreGetOnly()
        {
            var testCode = @"
public class Foo
{
    public int Bar { get; } = 1;
}";

            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task IgnoreExpressionBody()
        {
            var testCode = @"
public class Foo
{
    public int Bar => 1;
}";

            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task IgnoreCalculatedBody()
        {
            var testCode = @"
public class Foo
{
    public int Bar
    {
        get { return 1; }
    }
}";

            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task IgnoreAbstract()
        {
            var testCode = @"
public abstract class Foo
{
    public abstract int Bar { get; set; }
}";

            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task IgnoreStatic()
        {
            // maybe this should notify?
            var testCode = @"
public class Foo
{
    public static int Bar { get; set; }
}";

            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task IgnoreInternalClass()
        {
            // maybe this should notify?
            var testCode = @"
internal class Foo
{
    public int Bar { get; set; }
}";

            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task IgnoreInternalProperty()
        {
            // maybe this should notify?
            var testCode = @"
public class Foo
{
    internal int Bar { get; set; }
}";

            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task IgnoreDependencyProperty()
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
        ""Bar"", typeof(int), typeof(FooControl), new PropertyMetadata(default(int)));

    public int Bar
    {
        get { return (int) this.GetValue(BarProperty); }
        set { this.SetValue(BarProperty, value); }
    }
}";
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task IgnoresEvent()
        {
            var testCode = @"
using System;

public class Foo
{
    public event EventHandler foo;
}";
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task IgnoresMarkupExtension()
        {
            var testCode = @"
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

public class BooleanToVisibilityConverter : MarkupExtension, IValueConverter
{
    public Visibility WhenTrue { get; set; } = Visibility.Visible;

    public Visibility WhenFalse { get; set; } = Visibility.Collapsed;

    public Visibility WhenNull { get; set; } = Visibility.Collapsed;

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }

    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return this.WhenNull;
        }

        if (Equals(value, true))
        {
            return this.WhenTrue;
        }

        if (Equals(value, false))
        {
            return this.WhenFalse;
        }

        throw new ArgumentOutOfRangeException(nameof(value), value, ""Expected value to be of type bool or Nullable<bool>"");
    }

    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException($""{nameof(BooleanToVisibilityConverter)} is only for OneWay bindings"");
    }
}";
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task IgnoresDataTemplateSelector()
        {
            var testCode = @"
using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

[SuppressMessage(""ReSharper"", ""MemberCanBePrivate.Global"", Justification = ""Used from xaml"")]
public class DialogButtonTemplateSelector : DataTemplateSelector
{
    public DataTemplate OKTemplate { get; set; }

    public DataTemplate CancelTemplate { get; set; }

    public DataTemplate YesTemplate { get; set; }

    public DataTemplate NoTemplate { get; set; }

    /// <inheritdoc />
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        var result = item as MessageBoxResult?;
        if (!result.HasValue)
        {
            return base.SelectTemplate(item, container);
        }

        switch (result.Value)
        {
            case MessageBoxResult.None:
                return base.SelectTemplate(item, container);
            case MessageBoxResult.OK:
                return this.OKTemplate;
            case MessageBoxResult.Cancel:
                return this.CancelTemplate;
            case MessageBoxResult.Yes:
                return this.YesTemplate;
            case MessageBoxResult.No:
                return this.NoTemplate;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}";
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }
    }
}