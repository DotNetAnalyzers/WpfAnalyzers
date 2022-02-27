namespace WpfAnalyzers.Test.WPF0151UseNameofInsteadOfConstantTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class CodeFix
{
    private static readonly UseNameofFix Fix = new();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0151UseNameofInsteadOfConstant);

    [Test]
    public static void RoutedCommand()
    {
        var before = @"
namespace N
{
    using System.Windows.Input;

    public static class Foo
    {
	    const string BarName = ""Bar"";
        public static readonly RoutedCommand Bar = new RoutedCommand(↓BarName, typeof(Foo));
    }
}";

        var after = @"
namespace N
{
    using System.Windows.Input;

    public static class Foo
    {
	    const string BarName = ""Bar"";
        public static readonly RoutedCommand Bar = new RoutedCommand(nameof(Bar), typeof(Foo));
    }
}";
        RoslynAssert.CodeFix(new RoutedCommandCreationAnalyzer(), Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void RoutedUICommand()
    {
        var before = @"
namespace N
{
    using System.Windows.Input;

    public static class Foo
    {
	    const string BarName = ""Bar"";
        public static readonly RoutedUICommand Bar = new RoutedUICommand(""Some text"", ↓BarName, typeof(Foo));
    }
}";

        var after = @"
namespace N
{
    using System.Windows.Input;

    public static class Foo
    {
	    const string BarName = ""Bar"";
        public static readonly RoutedUICommand Bar = new RoutedUICommand(""Some text"", nameof(Bar), typeof(Foo));
    }
}";
        RoslynAssert.CodeFix(new RoutedCommandCreationAnalyzer(), Fix, ExpectedDiagnostic, before, after);
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
	    const string BarName = ""Bar"";
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(↓BarName, typeof(int), typeof(FooControl), new PropertyMetadata(default(int)));

        public int Bar
        {
            get => (int)GetValue(BarProperty);
            set => SetValue(BarProperty, value);
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
	    const string BarName = ""Bar"";
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(nameof(Bar), typeof(int), typeof(FooControl), new PropertyMetadata(default(int)));

        public int Bar
        {
            get => (int)GetValue(BarProperty);
            set => SetValue(BarProperty, value);
        }
    }
}";
        RoslynAssert.CodeFix(new RegistrationAnalyzer(), Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void DependencyPropertyRegisterArgumentPerLine()
    {
        var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
	    const string BarName = ""Bar"";

        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            ↓BarName,
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public int Bar
        {
            get => (int)GetValue(BarProperty);
            set => SetValue(BarProperty, value);
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
	    const string BarName = ""Bar"";

        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            nameof(Bar),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public int Bar
        {
            get => (int)GetValue(BarProperty);
            set => SetValue(BarProperty, value);
        }
    }
}";
        RoslynAssert.CodeFix(new RegistrationAnalyzer(), Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void RegisterRoutedEvent()
    {
        var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
	    const string ValueChangedName = ""ValueChanged"";

        /// <summary>Identifies the <see cref=""ValueChanged""/> event</summary>
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
            ↓ValueChangedName,
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(FooControl));

        public event RoutedEventHandler ValueChanged
        {
            add { this.AddHandler(ValueChangedEvent, value); }
            remove { this.RemoveHandler(ValueChangedEvent, value); }
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
	    const string ValueChangedName = ""ValueChanged"";

        /// <summary>Identifies the <see cref=""ValueChanged""/> event</summary>
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(ValueChanged),
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(FooControl));

        public event RoutedEventHandler ValueChanged
        {
            add { this.AddHandler(ValueChangedEvent, value); }
            remove { this.RemoveHandler(ValueChangedEvent, value); }
        }
    }
}";
        RoslynAssert.CodeFix(new RoutedEventBackingFieldOrPropertyAnalyzer(), Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void DependsOnWhenConstLiteral()
    {
        var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Markup;

    public class WithDependsOn : FrameworkElement
    {
	    const string Wrong = ""Value2"";

        public static readonly DependencyProperty Value1Property = DependencyProperty.Register(
            nameof(Value1),
            typeof(string),
            typeof(WithDependsOn));

        public static readonly DependencyProperty Value2Property = DependencyProperty.Register(
            nameof(Value2),
            typeof(string),
            typeof(WithDependsOn));


        [DependsOn(↓Wrong)]
        public string Value1
        {
            get => (string)this.GetValue(Value1Property);
            set => this.SetValue(Value1Property, value);
        }

        public string Value2
        {
            get => (string)this.GetValue(Value2Property);
            set => this.SetValue(Value2Property, value);
        }
    }
}";

        var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Markup;

    public class WithDependsOn : FrameworkElement
    {
	    const string Wrong = ""Value2"";

        public static readonly DependencyProperty Value1Property = DependencyProperty.Register(
            nameof(Value1),
            typeof(string),
            typeof(WithDependsOn));

        public static readonly DependencyProperty Value2Property = DependencyProperty.Register(
            nameof(Value2),
            typeof(string),
            typeof(WithDependsOn));


        [DependsOn(nameof(Value2))]
        public string Value1
        {
            get => (string)this.GetValue(Value1Property);
            set => this.SetValue(Value1Property, value);
        }

        public string Value2
        {
            get => (string)this.GetValue(Value2Property);
            set => this.SetValue(Value2Property, value);
        }
    }
}";
        RoslynAssert.CodeFix(new AttributeAnalyzer(), Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void DependsOnWhenConstNameof()
    {
        var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Markup;

    public class WithDependsOn : FrameworkElement
    {
	    const string Wrong = nameof(Value2);

        public static readonly DependencyProperty Value1Property = DependencyProperty.Register(
            nameof(Value1),
            typeof(string),
            typeof(WithDependsOn));

        public static readonly DependencyProperty Value2Property = DependencyProperty.Register(
            nameof(Value2),
            typeof(string),
            typeof(WithDependsOn));


        [DependsOn(↓Wrong)]
        public string Value1
        {
            get => (string)this.GetValue(Value1Property);
            set => this.SetValue(Value1Property, value);
        }

        public string Value2
        {
            get => (string)this.GetValue(Value2Property);
            set => this.SetValue(Value2Property, value);
        }
    }
}";

        var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Markup;

    public class WithDependsOn : FrameworkElement
    {
	    const string Wrong = nameof(Value2);

        public static readonly DependencyProperty Value1Property = DependencyProperty.Register(
            nameof(Value1),
            typeof(string),
            typeof(WithDependsOn));

        public static readonly DependencyProperty Value2Property = DependencyProperty.Register(
            nameof(Value2),
            typeof(string),
            typeof(WithDependsOn));


        [DependsOn(nameof(Value2))]
        public string Value1
        {
            get => (string)this.GetValue(Value1Property);
            set => this.SetValue(Value1Property, value);
        }

        public string Value2
        {
            get => (string)this.GetValue(Value2Property);
            set => this.SetValue(Value2Property, value);
        }
    }
}";
        RoslynAssert.CodeFix(new AttributeAnalyzer(), Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void ContentProperty()
    {
        var before = @"
namespace N
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;
    using System.Windows.Markup;

    [ContentProperty(↓ConvertersName)]
    [ValueConversion(typeof(object), typeof(object))]
    public class ValueConverterGroup : IValueConverter
    {
	    const string ConvertersName = ""Converters"";

        public List<IValueConverter> Converters { get; } = new List<IValueConverter>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => this.Converters
                   .Aggregate(value, (current, converter) => converter.Convert(current, targetType, parameter, culture));

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}";

        var after = @"
namespace N
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;
    using System.Windows.Markup;

    [ContentProperty(nameof(Converters))]
    [ValueConversion(typeof(object), typeof(object))]
    public class ValueConverterGroup : IValueConverter
    {
	    const string ConvertersName = ""Converters"";

        public List<IValueConverter> Converters { get; } = new List<IValueConverter>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => this.Converters
                   .Aggregate(value, (current, converter) => converter.Convert(current, targetType, parameter, culture));

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}";
        RoslynAssert.CodeFix(new AttributeAnalyzer(), Fix, ExpectedDiagnostic, before, after);
    }
}