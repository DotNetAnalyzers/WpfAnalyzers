namespace WpfAnalyzers
{
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class FieldOrPropertyExt
    {
        internal static bool TryGetAssignedValue(this FieldOrProperty fieldOrProperty, CancellationToken cancellationToken, out ExpressionSyntax value)
        {
            value = null;
            if (fieldOrProperty.Symbol is IFieldSymbol field)
            {
                return field.TryGetAssignedValue(cancellationToken, out value);
            }

            if (fieldOrProperty.Symbol is IPropertySymbol property)
            {
                if (property.TrySingleDeclaration(cancellationToken, out PropertyDeclarationSyntax declaration))
                {
                    if (declaration.Initializer != null)
                    {
                        value = declaration.Initializer.Value;
                    }
                    else if (declaration.ExpressionBody != null)
                    {
                        value = declaration.ExpressionBody.Expression;
                    }

                    return value != null;
                }
            }

            return false;
        }

        internal static bool IsStaticReadOnly(this FieldOrProperty fieldOrProperty)
        {
            if (fieldOrProperty.IsStatic)
            {
                switch (fieldOrProperty.Symbol)
                {
                    case IFieldSymbol field:
                        return field.IsReadOnly;
                    case IPropertySymbol property:
                        return property.IsGetOnly();
                }
            }

            return false;
        }
    }
}
