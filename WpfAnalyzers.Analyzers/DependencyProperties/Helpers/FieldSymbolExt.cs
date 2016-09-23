namespace WpfAnalyzers.DependencyProperties
{
    using Microsoft.CodeAnalysis;

    internal static class FieldSymbolExt
    {
        private const string DependencyPropertyTypeName = "DependencyProperty";

        internal static bool IsDependencyPropertyField(this IFieldSymbol fieldSymbol)
        {
            return fieldSymbol.Type.Name == DependencyPropertyTypeName;
        }
    }
}
