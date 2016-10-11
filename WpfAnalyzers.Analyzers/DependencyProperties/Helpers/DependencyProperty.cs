namespace WpfAnalyzers.DependencyProperties
{
    using System;
    using System.Threading;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class DependencyProperty
    {
        internal static bool TryGetRegisteredName(IFieldSymbol field, SemanticModel semanticModel, CancellationToken cancellationToken, out string result)
        {
            result = null;
            ExpressionSyntax value;
            if (field.TryGetAssignedValue(cancellationToken, out value))
            {
                var valueSymbol = semanticModel.SemanticModelFor(value)
                                               .GetSymbolInfo(value, cancellationToken)
                                               .Symbol;
                if (valueSymbol == null)
                {
                    return false;
                }

                var invocation = value as InvocationExpressionSyntax;
                ArgumentSyntax nameArg;
                if (invocation != null &&
                    valueSymbol.ContainingType.Name == Names.DependencyProperty &&
                    valueSymbol.Name.StartsWith("Register", StringComparison.Ordinal) &&
                    invocation.TryGetArgumentAtIndex(0, out nameArg))
                {
                    return nameArg.TryGetStringValue(semanticModel, cancellationToken, out result);
                }

                IFieldSymbol keyField;
                if (TryGetDependencyPropertyKeyField(field, semanticModel, cancellationToken, out keyField))
                {
                    return TryGetRegisteredName(keyField, semanticModel, cancellationToken, out result);
                }
            }

            return false;
        }

        internal static bool TryGetDependencyPropertyKeyField(IFieldSymbol field, SemanticModel semanticModel, CancellationToken cancellationToken, out IFieldSymbol result)
        {
            result = null;
            ExpressionSyntax value;
            if (field.TryGetAssignedValue(cancellationToken, out value))
            {
                var valueSymbol = semanticModel.SemanticModelFor(value)
                               .GetSymbolInfo(value, cancellationToken)
                               .Symbol;
                if (valueSymbol == null)
                {
                    return false;
                }

                var memberAccess = value as MemberAccessExpressionSyntax;
                if (memberAccess != null &&
                    valueSymbol.ContainingType.Name == Names.DependencyPropertyKey &&
                    valueSymbol.Name == Names.DependencyProperty)
                {
                    result = semanticModel.SemanticModelFor(memberAccess.Expression)
                                                .GetSymbolInfo(memberAccess.Expression, cancellationToken)
                                                .Symbol as IFieldSymbol;
                    return result != null;
                }
            }

            return false;
        }
    }
}