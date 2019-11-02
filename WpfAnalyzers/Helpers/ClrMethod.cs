namespace WpfAnalyzers
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class ClrMethod
    {
        /// <summary>
        /// Check if <paramref name="method"/> is a potential accessor for an attached property.
        /// </summary>
        internal static bool IsPotentialClrSetMethod(IMethodSymbol method, Compilation compilation)
        {
            return method != null &&
                   method.IsStatic &&
                   method.ReturnsVoid &&
                   method.Name.StartsWith("Set", StringComparison.Ordinal) &&
                   method.Parameters.Length == 2 &&
                   method.Parameters.TryElementAt(0, out var parameter) &&
                   parameter.Type.IsAssignableTo(KnownSymbols.DependencyObject, compilation);
        }

        internal static bool IsAttachedSet(IMethodSymbol method, SemanticModel semanticModel, CancellationToken cancellationToken, out BackingFieldOrProperty setField)
        {
            setField = default;
            if (!IsPotentialClrSetMethod(method, semanticModel.Compilation))
            {
                return false;
            }

            if (method.DeclaringSyntaxReferences.TrySingle(out var reference))
            {
                var methodDeclaration = reference.GetSyntax(cancellationToken) as MethodDeclarationSyntax;
                return IsAttachedSet(methodDeclaration, semanticModel, cancellationToken, out _, out setField);
            }

            return false;
        }

        /// <summary>
        /// Check if <paramref name="method"/> is a potential accessor for an attached property.
        /// </summary>
        internal static bool IsPotentialClrGetMethod(IMethodSymbol method, Compilation compilation)
        {
            return method != null &&
                   method.IsStatic &&
                   !method.ReturnsVoid &&
                   method.Name.StartsWith("Get", StringComparison.Ordinal) &&
                   method.Parameters.TrySingle(out var parameter) &&
                   parameter.Type.IsAssignableTo(KnownSymbols.DependencyObject, compilation);
        }

        internal static bool IsAttachedGet(IMethodSymbol method, SemanticModel semanticModel, CancellationToken cancellationToken, out BackingFieldOrProperty getField)
        {
            getField = default;
            return IsPotentialClrGetMethod(method, semanticModel.Compilation) &&
                   method.TrySingleMethodDeclaration(cancellationToken, out var declaration) &&
                   IsAttachedGet(declaration, semanticModel, cancellationToken, out _, out getField);
        }

        internal static bool IsAttachedSet(MethodDeclarationSyntax method, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out InvocationExpressionSyntax? setValueCall, out BackingFieldOrProperty setField)
        {
            setValueCall = null;
            setField = default;
            if (method == null ||
                method.ParameterList.Parameters.Count != 2 ||
                !method.ReturnType.IsVoid() ||
                !method.Modifiers.Any(SyntaxKind.StaticKeyword))
            {
                return false;
            }

            using (var walker = ClrSetterWalker.Borrow(semanticModel, method, cancellationToken))
            {
                if (!walker.IsSuccess)
                {
                    return false;
                }

                setValueCall = walker.SetValue ?? walker.SetCurrentValue;
                if (setValueCall?.Expression is MemberAccessExpressionSyntax memberAccess &&
                    memberAccess.IsKind(SyntaxKind.SimpleMemberAccessExpression) &&
                    memberAccess.Expression is IdentifierNameSyntax member)
                {
                    if (method.ParameterList.Parameters[0].Identifier.ValueText != member.Identifier.ValueText)
                    {
                        return false;
                    }

                    if (setValueCall.TryGetArgumentAtIndex(1, out var arg) &&
                        method.ParameterList.Parameters.TryElementAt(1, out var parameter) &&
                        walker.Property is { } property &&
                        property.Expression is {} expression)
                    {
                        if (arg.Expression is IdentifierNameSyntax argIdentifier &&
                            argIdentifier.Identifier.ValueText == parameter.Identifier.ValueText)
                        {
                            return semanticModel.TryGetSymbol(expression, cancellationToken, out var symbol) &&
                                   BackingFieldOrProperty.TryCreateForDependencyProperty(symbol, out setField);
                        }

                        if (arg.Expression is InvocationExpressionSyntax invocation &&
                            invocation.TrySingleArgument(out var nestedArg) &&
                            nestedArg.Expression is IdentifierNameSyntax nestedArgId &&
                            nestedArgId.Identifier.ValueText == parameter.Identifier.ValueText)
                        {
                            return semanticModel.TryGetSymbol(expression, cancellationToken, out var symbol) &&
                                   BackingFieldOrProperty.TryCreateForDependencyProperty(symbol, out setField);
                        }

                        if (arg.Expression is ConditionalExpressionSyntax ternary &&
                            ternary.Condition is IdentifierNameSyntax conditionIdentifier &&
                            conditionIdentifier.Identifier.ValueText == parameter.Identifier.ValueText)
                        {
                            return semanticModel.TryGetSymbol(expression, cancellationToken, out var symbol) &&
                                   BackingFieldOrProperty.TryCreateForDependencyProperty(symbol, out setField);
                        }
                    }
                }

                return false;
            }
        }

        internal static bool IsAttachedGet(MethodDeclarationSyntax method, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out InvocationExpressionSyntax? call, out BackingFieldOrProperty getField)
        {
            call = null;
            getField = default;
            if (method == null ||
                method.ParameterList.Parameters.Count != 1 ||
                method.ReturnType.IsVoid() ||
                !method.Modifiers.Any(SyntaxKind.StaticKeyword))
            {
                return false;
            }

            using (var walker = ClrGetterWalker.Borrow(semanticModel, method, cancellationToken))
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

                return BackingFieldOrProperty.TryCreateForDependencyProperty(semanticModel.GetSymbolSafe(walker.Property.Expression, cancellationToken), out getField);
            }
        }
    }
}
