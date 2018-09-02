// ReSharper disable InconsistentNaming
namespace WpfAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;

    internal static class KnownSymbol
    {
        internal static readonly ObjectType Object = new ObjectType();
        internal static readonly QualifiedType Boolean = Create("System.Boolean", "bool");
        internal static readonly QualifiedType IServiceProvider = Create("System.IServiceProvider");

        internal static readonly DependencyObjectType DependencyObject = new DependencyObjectType();
        internal static readonly FrameworkElementType FrameworkElement = new FrameworkElementType();
        internal static readonly EventManagerType EventManager = new EventManagerType();
        internal static readonly DependencyPropertyType DependencyProperty = new DependencyPropertyType();
        internal static readonly DependencyPropertyKeyType DependencyPropertyKey = new DependencyPropertyKeyType();
        internal static readonly QualifiedType UIPropertyMetadata = Create("System.Windows.UIPropertyMetadata");
        internal static readonly QualifiedType PropertyMetadata = Create("System.Windows.PropertyMetadata");
        internal static readonly QualifiedType FrameworkPropertyMetadata = Create("System.Windows.FrameworkPropertyMetadata");
        internal static readonly QualifiedType DependencyPropertyChangedEventArgs = Create("System.Windows.DependencyPropertyChangedEventArgs");
        internal static readonly QualifiedType RoutedEvent = Create("System.Windows.RoutedEvent");
        internal static readonly QualifiedType TemplatePartAttribute = Create("System.Windows.TemplatePartAttribute");

        internal static readonly QualifiedType PropertyChangedCallback = Create("System.Windows.PropertyChangedCallback");
        internal static readonly QualifiedType CoerceValueCallback = Create("System.Windows.CoerceValueCallback");
        internal static readonly QualifiedType ValidateValueCallback = Create("System.Windows.ValidateValueCallback");

        internal static readonly QualifiedType RoutedCommand = Create("System.Windows.Input.RoutedCommand");
        internal static readonly QualifiedType RoutedUICommand = Create("System.Windows.Input.RoutedUICommand");

        internal static readonly QualifiedType FontFamily = Create("System.Windows.Media.FontFamily");
        internal static readonly QualifiedType Freezable = Create("System.Windows.Freezable");
        internal static readonly QualifiedType MarkupExtension = Create("System.Windows.Markup.MarkupExtension");
        internal static readonly QualifiedType MarkupExtensionReturnTypeAttribute = Create("System.Windows.Markup.MarkupExtensionReturnTypeAttribute");
        internal static readonly QualifiedType ConstructorArgumentAttribute = Create("System.Windows.Markup.ConstructorArgumentAttribute");
        internal static readonly QualifiedType AttachedPropertyBrowsableForTypeAttribute = Create("System.Windows.AttachedPropertyBrowsableForTypeAttribute");
        internal static readonly QualifiedType IValueConverter = Create("System.Windows.Data.IValueConverter");
        internal static readonly QualifiedType IMultiValueConverter = Create("System.Windows.Data.IMultiValueConverter");

        internal static readonly XmlnsPrefixAttributeType XmlnsPrefixAttribute = new XmlnsPrefixAttributeType();
        internal static readonly XmlnsDefinitionAttributeType XmlnsDefinitionAttribute = new XmlnsDefinitionAttributeType();
        internal static readonly QualifiedType ValueConversionAttribute = new QualifiedType("System.Windows.Data.ValueConversionAttribute");

        private static QualifiedType Create(string qualifiedName, string alias = null)
        {
            return new QualifiedType(qualifiedName, alias);
        }
    }
}
