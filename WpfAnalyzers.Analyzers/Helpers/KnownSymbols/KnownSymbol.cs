namespace WpfAnalyzers
{
    internal static class KnownSymbol
    {
        internal static readonly QualifiedType Object = Create("System.Object");

        internal static readonly DependencyObjectType DependencyObject = new DependencyObjectType();
        internal static readonly DependencyPropertyType DependencyProperty = new DependencyPropertyType();
        internal static readonly DependencyPropertyKeyType DependencyPropertyKey = new DependencyPropertyKeyType();
        internal static readonly QualifiedType PropertyMetadata = Create("System.Windows.PropertyMetadata");

        internal static readonly QualifiedType PropertyChangedCallback = Create("System.Windows.PropertyChangedCallback");
        internal static readonly QualifiedType CoerceValueCallback = Create("System.Windows.CoerceValueCallback");

        internal static readonly QualifiedType Freezable = Create("System.Windows.Freezable");

        internal static readonly QualifiedType XmlnsPrefixAttribute = Create("System.Windows.Markup.XmlnsPrefixAttribute");
        internal static readonly QualifiedType XmlnsDefinitionAttribute = Create("System.Windows.Markup.XmlnsDefinitionAttribute");

        private static QualifiedType Create(string qualifiedName)
        {
            return new QualifiedType(qualifiedName);
        }
    }
}