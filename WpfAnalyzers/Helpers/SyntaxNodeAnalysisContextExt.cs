namespace WpfAnalyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

internal static class SyntaxNodeAnalysisContextExt
{
    internal static IPropertySymbol? ContainingProperty(this SyntaxNodeAnalysisContext context)
    {
        var containingSymbol = context.ContainingSymbol;
        if (containingSymbol is IPropertySymbol propertySymbol)
        {
            return propertySymbol;
        }

        return (containingSymbol as IMethodSymbol)?.AssociatedSymbol as IPropertySymbol;
    }
}