namespace WpfAnalyzers;

using System.Threading;
using Gu.Roslyn.AnalyzerExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class FieldOrPropertyExt
{
    internal static ExpressionSyntax? Value(this FieldOrProperty fieldOrProperty, CancellationToken cancellationToken)
    {
        if (fieldOrProperty.Symbol is IFieldSymbol field)
        {
            return field.Value(cancellationToken);
        }

        if (fieldOrProperty.Symbol is IPropertySymbol property)
        {
            if (property.TrySingleDeclaration(cancellationToken, out PropertyDeclarationSyntax? declaration))
            {
                return declaration switch
                {
                    { Initializer: { } initializer } => initializer.Value,
                    { ExpressionBody: { } expressionBody } => expressionBody.Expression,
                    _ => null,
                };
            }
        }

        return null;
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