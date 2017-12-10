namespace WpfAnalyzers
{
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class ClrMethod
    {
        /// <summary>
        /// Check if <paramref name="method"/> is a potential accessor for an attached property
        /// </summary>
        internal static bool IsPotentialClrSetMethod(IMethodSymbol method)
        {
            if (method == null)
            {
                return false;
            }

            if (!method.IsStatic ||
                method.Parameters.Length != 2 ||
                method.AssociatedSymbol != null ||
                !method.Parameters[0].Type.Is(KnownSymbol.DependencyObject) ||
                method.Parameters[1].Type == KnownSymbol.DependencyPropertyChangedEventArgs ||
                !method.ReturnsVoid ||
                !method.Name.StartsWith("Set"))
            {
                return false;
            }

            return true;
        }

        internal static bool IsAttachedSetMethod(IMethodSymbol method, SemanticModel semanticModel, CancellationToken cancellationToken, out BackingFieldOrProperty setField)
        {
            setField = default(BackingFieldOrProperty);
            if (!IsPotentialClrSetMethod(method))
            {
                return false;
            }

            if (method.DeclaringSyntaxReferences.TryGetSingle(out var reference))
            {
                var methodDeclaration = reference.GetSyntax(cancellationToken) as MethodDeclarationSyntax;
                return IsAttachedSetMethod(methodDeclaration, semanticModel, cancellationToken, out _, out setField);
            }

            return false;
        }

        /// <summary>
        /// Check if <paramref name="method"/> is a potential accessor for an attached property
        /// </summary>
        internal static bool IsPotentialClrGetMethod(IMethodSymbol method)
        {
            if (method == null)
            {
                return false;
            }

            if (!method.IsStatic ||
                method.Parameters.Length != 1 ||
                !method.Parameters[0].Type.Is(KnownSymbol.DependencyObject) ||
                method.ReturnsVoid ||
                !method.Name.StartsWith("Get"))
            {
                return false;
            }

            return true;
        }

        internal static bool IsAttachedGetMethod(IMethodSymbol method, SemanticModel semanticModel, CancellationToken cancellationToken, out BackingFieldOrProperty getField)
        {
            getField = default(BackingFieldOrProperty);
            if (!IsPotentialClrGetMethod(method))
            {
                return false;
            }

            if (method.DeclaringSyntaxReferences.TryGetSingle(out var reference))
            {
                var methodDeclaration = reference.GetSyntax(cancellationToken) as MethodDeclarationSyntax;
                return IsAttachedGetMethod(methodDeclaration, semanticModel, cancellationToken, out _, out getField);
            }

            return false;
        }

        internal static bool IsAttachedSetMethod(MethodDeclarationSyntax method, SemanticModel semanticModel, CancellationToken cancellationToken, out InvocationExpressionSyntax setValueCall, out BackingFieldOrProperty setField)
        {
            setValueCall = null;
            setField = default(BackingFieldOrProperty);
            if (method == null ||
                method.ParameterList.Parameters.Count != 2 ||
                !method.ReturnType.IsVoid() ||
                !method.Modifiers.Any(SyntaxKind.StaticKeyword))
            {
                return false;
            }

            using (var walker = ClrSetterWalker.Borrow(semanticModel, cancellationToken, method))
            {
                if (!walker.IsSuccess)
                {
                    return false;
                }

                setValueCall = walker.SetValue ?? walker.SetCurrentValue;
                if (setValueCall.Expression is MemberAccessExpressionSyntax memberAccess &&
                    memberAccess.IsKind(SyntaxKind.SimpleMemberAccessExpression) &&
                    memberAccess.Expression is IdentifierNameSyntax member)
                {
                    if (method.ParameterList.Parameters[0].Identifier.ValueText != member.Identifier.ValueText)
                    {
                        return false;
                    }

                    if (setValueCall.TryGetArgumentAtIndex(1, out var arg) &&
                        method.ParameterList.Parameters.TryGetAtIndex(1, out var parameter))
                    {
                        if (arg.Expression is IdentifierNameSyntax argIdentifier &&
                            argIdentifier.Identifier.ValueText == parameter.Identifier.ValueText)
                        {
                            return BackingFieldOrProperty.TryCreate(semanticModel.GetSymbolSafe(walker.Property.Expression, cancellationToken), out setField);
                        }

                        if (arg.Expression is InvocationExpressionSyntax invocation &&
                            invocation.TryGetSingleArgument(out var nestedArg) &&
                            nestedArg.Expression is IdentifierNameSyntax nestedArgId &&
                            nestedArgId.Identifier.ValueText == parameter.Identifier.ValueText)
                        {
                            return BackingFieldOrProperty.TryCreate(semanticModel.GetSymbolSafe(walker.Property.Expression, cancellationToken), out setField);
                        }

                        if (arg.Expression is ConditionalExpressionSyntax ternary &&
                            ternary.Condition is IdentifierNameSyntax conditionIdentifier &&
                            conditionIdentifier.Identifier.ValueText == parameter.Identifier.ValueText)
                        {
                            return BackingFieldOrProperty.TryCreate(semanticModel.GetSymbolSafe(walker.Property.Expression, cancellationToken), out setField);
                        }
                    }
                }

                return false;
            }
        }

        internal static bool IsAttachedGetMethod(MethodDeclarationSyntax method, SemanticModel semanticModel, CancellationToken cancellationToken, out InvocationExpressionSyntax call, out BackingFieldOrProperty getField)
        {
            call = null;
            getField = default(BackingFieldOrProperty);
            if (method == null ||
                method.ParameterList.Parameters.Count != 1 ||
                method.ReturnType.IsVoid() ||
                !method.Modifiers.Any(SyntaxKind.StaticKeyword))
            {
                return false;
            }

            using (var walker = ClrGetterWalker.Borrow(semanticModel, cancellationToken, method))
            {
                call = walker.GetValue;
                var memberAccess = walker.GetValue?.Expression as MemberAccessExpressionSyntax;
                var member = memberAccess?.Expression as IdentifierNameSyntax;
                if (memberAccess == null ||
                    member == null ||
                    !memberAccess.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                {
                    return false;
                }

                if (method.ParameterList.Parameters[0].Identifier.ValueText != member.Identifier.ValueText)
                {
                    return false;
                }

                return BackingFieldOrProperty.TryCreate(semanticModel.GetSymbolSafe(walker.Property.Expression, cancellationToken), out getField);
            }
        }
    }
}