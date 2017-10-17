namespace WpfAnalyzers
{
    using System;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class DependencyProperty
    {
        internal static bool IsPotentialDependencyPropertyBackingField(IFieldSymbol field)
        {
            return field != null &&
                   field.Type == KnownSymbol.DependencyProperty &&
                   field.IsReadOnly &&
                   field.IsStatic;
        }

        internal static bool IsPotentialDependencyPropertyKeyBackingField(IFieldSymbol field)
        {
            return field != null &&
                   field.Type == KnownSymbol.DependencyPropertyKey &&
                   field.IsReadOnly &&
                   field.IsStatic;
        }

        internal static ArgumentSyntax CreateArgument(IFieldSymbol field, SemanticModel semanticModel, int position)
        {
            if (semanticModel.LookupStaticMembers(position).Contains(field))
            {
                return SyntaxFactory.Argument(SyntaxFactory.IdentifierName(field.Name));
            }

            var typeName = field.ContainingType.ToMinimalDisplayString(semanticModel, position, SymbolDisplayFormat.MinimallyQualifiedFormat);
            return SyntaxFactory.Argument(SyntaxFactory.ParseExpression($"{typeName}.{field.Name}"));
        }

        internal static bool TryGetRegisteredName(IFieldSymbol field, SemanticModel semanticModel, CancellationToken cancellationToken, out string result)
        {
            result = null;
            if (field == null)
            {
                return false;
            }

            if (TryGetRegisterInvocationRecursive(field, semanticModel, cancellationToken, out InvocationExpressionSyntax invocation))
            {
                if (invocation.TryGetArgumentAtIndex(0, out ArgumentSyntax arg))
                {
                    return arg.TryGetStringValue(semanticModel, cancellationToken, out result);
                }

                return false;
            }

            if (TryGetPropertyByName(field, out IPropertySymbol property))
            {
                result = property.Name;
                return true;
            }

            return false;
        }

        internal static bool TryGetRegisteredType(IFieldSymbol field, SemanticModel semanticModel, CancellationToken cancellationToken, out ITypeSymbol result)
        {
            result = null;
            if (TryGetRegisterInvocationRecursive(field, semanticModel, cancellationToken, out InvocationExpressionSyntax invocation))
            {
                if (invocation.TryGetArgumentAtIndex(1, out ArgumentSyntax arg))
                {
                    if (!arg.TryGetTypeofValue(semanticModel, cancellationToken, out result))
                    {
                        return false;
                    }

                    if (result.Kind == SymbolKind.TypeParameter)
                    {
                        var index = field.ContainingType.TypeParameters.IndexOf((ITypeParameterSymbol)result);
                        if (index < 0)
                        {
                            result = null;
                            return false;
                        }

                        result = field.ContainingType.TypeArguments[index];
                    }

                    return result != null;
                }

                return false;
            }

            if (TryGetPropertyByName(field, out IPropertySymbol property))
            {
                result = property.Type;
                return true;
            }

            return false;
        }

        internal static bool TryGetDependencyPropertyKeyField(IFieldSymbol field, SemanticModel semanticModel, CancellationToken cancellationToken, out IFieldSymbol result)
        {
            result = null;
            if (field.TryGetAssignedValue(cancellationToken, out ExpressionSyntax value))
            {
                var symbol = semanticModel.GetSymbolSafe(value, cancellationToken);
                if (symbol is IMethodSymbol method)
                {
                    if (method != KnownSymbol.DependencyProperty.AddOwner)
                    {
                        return false;
                    }

                    var invocation = (InvocationExpressionSyntax)value;
                    var member = invocation.Expression as MemberAccessExpressionSyntax;

                    field = semanticModel.GetSymbolSafe(member?.Expression, cancellationToken) as IFieldSymbol;
                    return TryGetDependencyPropertyKeyField(field, semanticModel, cancellationToken, out result);
                }

                var property = symbol as IPropertySymbol;
                if (property == null ||
                    property != KnownSymbol.DependencyPropertyKey.DependencyProperty)
                {
                    return false;
                }

                if (value is MemberAccessExpressionSyntax memberAccess)
                {
                    result = semanticModel.GetSymbolSafe(memberAccess.Expression, cancellationToken) as IFieldSymbol;
                    return result != null;
                }
            }

            return false;
        }

        internal static bool TryGetDependencyAddOwnerSourceField(IFieldSymbol field, SemanticModel semanticModel, CancellationToken cancellationToken, out IFieldSymbol result)
        {
            result = null;
            if (field.TryGetAssignedValue(cancellationToken, out ExpressionSyntax value))
            {
                var invocation = value as InvocationExpressionSyntax;
                if (invocation == null)
                {
                    return false;
                }

                var invocationSymbol = semanticModel.GetSymbolSafe(invocation, cancellationToken) as IMethodSymbol;
                if (invocationSymbol == KnownSymbol.DependencyProperty.AddOwner)
                {
                    var addOwner = (MemberAccessExpressionSyntax)invocation.Expression;
                    result = semanticModel.GetSymbolSafe(addOwner.Expression, cancellationToken) as IFieldSymbol;
                    return result != null;
                }
            }

            return false;
        }

        internal static bool TryGetRegisterInvocation(IFieldSymbol field, SemanticModel semanticModel, CancellationToken cancellationToken, out InvocationExpressionSyntax result)
        {
            result = null;
            if (field.TryGetAssignedValue(cancellationToken, out ExpressionSyntax value))
            {
                var invocation = value as InvocationExpressionSyntax;
                if (invocation == null)
                {
                    return false;
                }

                var invocationSymbol = semanticModel.GetSymbolSafe(invocation, cancellationToken);
                if (invocationSymbol == null)
                {
                    return false;
                }

                if (invocationSymbol.ContainingType == KnownSymbol.DependencyProperty &&
                    invocationSymbol.Name.StartsWith("Register", StringComparison.Ordinal))
                {
                    result = invocation;
                    return true;
                }
            }

            return false;
        }

        internal static bool TryGetRegisterInvocationRecursive(IFieldSymbol field, SemanticModel semanticModel, CancellationToken cancellationToken, out InvocationExpressionSyntax result)
        {
            if (TryGetDependencyPropertyKeyField(field, semanticModel, cancellationToken, out IFieldSymbol keyField))
            {
                return TryGetRegisterInvocationRecursive(keyField, semanticModel, cancellationToken, out result);
            }

            if (TryGetDependencyAddOwnerSourceField(field, semanticModel, cancellationToken, out IFieldSymbol addOwnerSource))
            {
                return TryGetRegisterInvocationRecursive(addOwnerSource, semanticModel, cancellationToken, out result);
            }

            return TryGetRegisterInvocation(field, semanticModel, cancellationToken, out result);
        }

        private static bool TryGetPropertyByName(IFieldSymbol field, out IPropertySymbol property)
        {
            property = null;

            if (IsPotentialDependencyPropertyBackingField(field) || IsPotentialDependencyPropertyKeyBackingField(field))
            {
                var suffix = IsPotentialDependencyPropertyBackingField(field)
                                 ? "Property"
                                 : "PropertyKey";

                foreach (var symbol in field.ContainingType.GetMembers())
                {
                    var candidate = symbol as IPropertySymbol;
                    if (candidate == null)
                    {
                        continue;
                    }

                    if (!field.Name.IsParts(candidate.Name, suffix))
                    {
                        continue;
                    }

                    if (property != null)
                    {
                        property = null;
                        return false;
                    }

                    property = symbol as IPropertySymbol;
                }
            }

            return property != null;
        }
    }
}