// ReSharper disable InconsistentNaming
namespace WpfAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;

    internal static class KnownSymbols
    {
        internal static readonly ObjectType Object = new();
        internal static readonly QualifiedType Boolean = Create("System.Boolean", "bool");
        internal static readonly QualifiedType IEnumerable = Create("System.Collections.IEnumerable");
        internal static readonly QualifiedType IServiceProvider = Create("System.IServiceProvider");

        internal static readonly DependencyObjectType DependencyObject = new();
        internal static readonly FrameworkElementType FrameworkElement = new();
        internal static readonly EventManagerType EventManager = new();
        internal static readonly DependencyPropertyType DependencyProperty = new();
        internal static readonly DependencyPropertyKeyType DependencyPropertyKey = new();
        internal static readonly QualifiedType Freezable = Create("System.Windows.Freezable");
        internal static readonly QualifiedType UIPropertyMetadata = Create("System.Windows.UIPropertyMetadata");
        internal static readonly QualifiedType PropertyMetadata = Create("System.Windows.PropertyMetadata");
        internal static readonly QualifiedType FrameworkPropertyMetadata = Create("System.Windows.FrameworkPropertyMetadata");
        internal static readonly QualifiedType DependencyPropertyChangedEventArgs = Create("System.Windows.DependencyPropertyChangedEventArgs");
        internal static readonly QualifiedType RoutedEvent = Create("System.Windows.RoutedEvent");
        internal static readonly QualifiedType Style = Create("System.Windows.Style");
        internal static readonly QualifiedType TemplatePartAttribute = Create("System.Windows.TemplatePartAttribute");
        internal static readonly QualifiedType AttachedPropertyBrowsableForTypeAttribute = Create("System.Windows.AttachedPropertyBrowsableForTypeAttribute");

        internal static readonly QualifiedType PropertyChangedCallback = Create("System.Windows.PropertyChangedCallback");
        internal static readonly QualifiedType CoerceValueCallback = Create("System.Windows.CoerceValueCallback");
        internal static readonly QualifiedType ValidateValueCallback = Create("System.Windows.ValidateValueCallback");
        internal static readonly QualifiedType ComponentResourceKey = Create("System.Windows.ComponentResourceKey");
        internal static readonly QualifiedType StyleTypedPropertyAttribute = Create("System.Windows.StyleTypedPropertyAttribute");

        internal static readonly QualifiedType RoutedCommand = Create("System.Windows.Input.RoutedCommand");
        internal static readonly QualifiedType RoutedUICommand = Create("System.Windows.Input.RoutedUICommand");

        internal static readonly QualifiedType FontFamily = Create("System.Windows.Media.FontFamily");

        internal static readonly QualifiedType MarkupExtension = Create("System.Windows.Markup.MarkupExtension");
        internal static readonly QualifiedType XamlSetMarkupExtensionAttribute = Create("System.Windows.Markup.XamlSetMarkupExtensionAttribute");
        internal static readonly QualifiedType XamlSetMarkupExtensionEventArgs = Create("System.Windows.Markup.XamlSetMarkupExtensionEventArgs");
        internal static readonly QualifiedType XamlSetTypeConverterAttribute = Create("System.Windows.Markup.XamlSetTypeConverterAttribute");
        internal static readonly QualifiedType XamlSetTypeConverterEventArgs = Create("System.Windows.Markup.XamlSetTypeConverterEventArgs");
        internal static readonly QualifiedType MarkupExtensionReturnTypeAttribute = Create("System.Windows.Markup.MarkupExtensionReturnTypeAttribute");
        internal static readonly QualifiedType ConstructorArgumentAttribute = Create("System.Windows.Markup.ConstructorArgumentAttribute");
        internal static readonly QualifiedType ContentPropertyAttribute = Create("System.Windows.Markup.ContentPropertyAttribute");
        internal static readonly QualifiedType DependsOnAttribute = Create("System.Windows.Markup.DependsOnAttribute");
        internal static readonly QualifiedType XmlnsPrefixAttribute = new("System.Windows.Markup.XmlnsPrefixAttribute");

        internal static readonly QualifiedType IValueConverter = Create("System.Windows.Data.IValueConverter");
        internal static readonly QualifiedType IMultiValueConverter = Create("System.Windows.Data.IMultiValueConverter");
        internal static readonly QualifiedType ValueConversionAttribute = new("System.Windows.Data.ValueConversionAttribute");

        internal static readonly XmlnsDefinitionAttributeType XmlnsDefinitionAttribute = new();

        private static QualifiedType Create(string qualifiedName, string? alias = null)
        {
            return new QualifiedType(qualifiedName, alias);
        }
    }
}
