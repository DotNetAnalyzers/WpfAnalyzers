namespace WpfAnalyzers.DependencyProperties
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
                   field.Type.Name == Names.DependencyProperty &&
                   field.IsReadOnly &&
                   field.IsStatic;
        }

        internal static bool IsPotentialDependencyPropertyKeyBackingField(IFieldSymbol field)
        {
            return field != null &&
                   field.Type.Name == Names.DependencyPropertyKey &&
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
            InvocationExpressionSyntax invocation;
            if (TryGetRegisterInvocationRecursive(field, semanticModel, cancellationToken, out invocation))
            {
                ArgumentSyntax arg;
                if (invocation.TryGetArgumentAtIndex(0, out arg))
                {
                    return arg.TryGetStringValue(semanticModel, cancellationToken, out result);
                }

                return false;
            }

            IPropertySymbol property;
            if (TryGetPropertyByName(field, out property))
            {
                result = property.Name;
                return true;
            }

            return false;
        }

        internal static bool TryGetRegisteredType(IFieldSymbol field, SemanticModel semanticModel, CancellationToken cancellationToken, out ITypeSymbol result)
        {
            result = null;
            InvocationExpressionSyntax invocation;
            if (TryGetRegisterInvocationRecursive(field, semanticModel, cancellationToken, out invocation))
            {
                ArgumentSyntax arg;
                if (invocation.TryGetArgumentAtIndex(1, out arg))
                {
                    return arg.TryGetTypeofValue(semanticModel, cancellationToken, out result);
                }

                return false;
            }

            IPropertySymbol property;
            if (TryGetPropertyByName(field, out property))
            {
                result = property.Type;
                return true;
            }

            return false;
        }

        internal static bool TryGetDependencyPropertyKeyField(IFieldSymbol field, SemanticModel semanticModel, CancellationToken cancellationToken, out IFieldSymbol result)
        {
            result = null;
            ExpressionSyntax value;
            if (field.TryGetAssignedValue(cancellationToken, out value))
            {
                var valueSymbol = ModelExtensions.GetSymbolInfo(semanticModel.SemanticModelFor(value), value, cancellationToken)
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
                    result = ModelExtensions.GetSymbolInfo(semanticModel.SemanticModelFor(memberAccess.Expression), memberAccess.Expression, cancellationToken)
                                                .Symbol as IFieldSymbol;
                    return result != null;
                }
            }

            return false;
        }

        internal static bool TryGetDependencyAddOwnerSourceField(IFieldSymbol field, SemanticModel semanticModel, CancellationToken cancellationToken, out IFieldSymbol result)
        {
            result = null;
            ExpressionSyntax value;
            if (field.TryGetAssignedValue(cancellationToken, out value))
            {
                var invocation = value as InvocationExpressionSyntax;
                if (invocation == null)
                {
                    return false;
                }

                var invocationSymbol = semanticModel.SemanticModelFor(invocation)
                                               .GetSymbolInfo(invocation, cancellationToken)
                                               .Symbol;
                if (invocationSymbol.ContainingType.Name == Names.DependencyProperty &&
                    invocationSymbol.Name == Names.AddOwner)
                {
                    var addOwner = (MemberAccessExpressionSyntax)invocation.Expression;
                    result = semanticModel.GetSymbolInfo(addOwner.Expression, cancellationToken).Symbol as IFieldSymbol;
                    return result != null;
                }
            }

            return false;
        }

        internal static bool TryGetRegisterInvocation(IFieldSymbol field, SemanticModel semanticModel, CancellationToken cancellationToken, out InvocationExpressionSyntax result)
        {
            result = null;
            ExpressionSyntax value;
            if (field.TryGetAssignedValue(cancellationToken, out value))
            {
                var invocation = value as InvocationExpressionSyntax;
                if (invocation == null)
                {
                    return false;
                }

                var invocationSymbol = semanticModel.SemanticModelFor(invocation)
                                               .GetSymbolInfo(invocation, cancellationToken)
                                               .Symbol;

                if (invocationSymbol.ContainingType.Name == Names.DependencyProperty &&
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
            result = null;
            IFieldSymbol keyField;
            if (TryGetDependencyPropertyKeyField(field, semanticModel, cancellationToken, out keyField))
            {
                return TryGetRegisterInvocationRecursive(keyField, semanticModel, cancellationToken, out result);
            }

            IFieldSymbol addOwnerSource;
            if (TryGetDependencyAddOwnerSourceField(field, semanticModel, cancellationToken, out addOwnerSource))
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