namespace WpfAnalyzers.DependencyProperties
{
    using Microsoft.CodeAnalysis;

    internal static class FieldSymbolExt
    {
        internal static bool IsDependencyPropertyField(this IFieldSymbol fieldSymbol)
        {
            return fieldSymbol.Type.Name == Names.DependencyProperty;
        }
    }
}
