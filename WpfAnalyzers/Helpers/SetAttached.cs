namespace WpfAnalyzers
{
    using System;
    using System.Threading;

    using Gu.Roslyn.AnalyzerExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal readonly struct SetAttached
    {
        internal readonly DependencyObject.SetValue SetValue;
        internal readonly BackingFieldOrProperty Backing;

        private SetAttached(DependencyObject.SetValue setValue, BackingFieldOrProperty backing)
        {
            this.SetValue = setValue;
            this.Backing = backing;
        }

        /// <summary>
        /// Check if <paramref name="method"/> is a potential accessor for an attached property.
        /// </summary>
        internal static bool CanMatch(IMethodSymbol method, Compilation compilation)
        {
            return method is { IsStatic: true, ReturnsVoid: true, Parameters: { Length: 2 } } &&
                   method.Name.StartsWith("Set", StringComparison.Ordinal) &&
                   method.Parameters.TryElementAt(0, out var parameter) &&
                   parameter.Type.IsAssignableTo(KnownSymbols.DependencyObject, compilation);
        }

        internal static SetAttached? Match(IMethodSymbol method, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (CanMatch(method, semanticModel.Compilation) &&
                method.TrySingleDeclaration(cancellationToken, out MethodDeclarationSyntax? methodDeclaration))
            {
                return Match(methodDeclaration, semanticModel, cancellationToken);
            }

            return null;
        }

        internal static SetAttached? Match(MethodDeclarationSyntax method, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (method is { Parent: TypeDeclarationSyntax containingType, ReturnType: PredefinedTypeSyntax { Keyword: { ValueText: "void" } }, ParameterList: { Parameters: { Count: 2 } parameters } } &&
                method.Modifiers.Any(SyntaxKind.StaticKeyword) &&
                DependencyObject.SetValue.Find(MethodOrAccessor.Create(method), semanticModel, cancellationToken) is { Invocation: { } invocation } getValue &&
                InvokedOnParameter(parameters[0], invocation) &&
                PassesParameter(invocation, parameters[1]) &&
                semanticModel.TryGetSymbol(getValue.PropertyArgument.Expression, cancellationToken, out var symbol) &&
                BackingFieldOrProperty.TryCreateForDependencyProperty(symbol, out var backing) &&
                backing.ContainingType.Name == containingType.Identifier.ValueText &&
                !HasBetterMatch(backing, method))
            {
                return new SetAttached(getValue, backing);
            }

            return null;

            static bool InvokedOnParameter(ParameterSyntax parameter, InvocationExpressionSyntax invocation)
            {
                return invocation switch
                {
                    { Expression: MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax identifierName } }
                        => identifierName.Identifier.ValueText == parameter.Identifier.ValueText,
                    _ => false,
                };
            }

            static bool PassesParameter(InvocationExpressionSyntax invocation, ParameterSyntax parameter)
            {
                return invocation switch
                {
                    { ArgumentList: { Arguments: { Count: 2 } arguments } }
                        => arguments[1] switch
                        {
                            { Expression: IdentifierNameSyntax identifierName } => identifierName.Identifier.ValueText == parameter.Identifier.ValueText,
                            { Expression: MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax identifierName } }
                                when identifierName.Identifier.ValueText == parameter.Identifier.ValueText
                                => false,
                            { Expression: { } expression } => UsesParameter(expression),
                            _ => false,
                        },

                    _ => false,
                };

                bool UsesParameter(ExpressionSyntax e)
                {
                    using var walker = SpecificIdentifierNameWalker.Borrow(e, parameter.Identifier.ValueText);
                    return walker.IdentifierNames.Count > 0;
                }
            }

            static bool HasBetterMatch(BackingFieldOrProperty backing, MethodDeclarationSyntax method)
            {
                if (method.Identifier.ValueText.IsParts("Set", backing.Name))
                {
                    return false;
                }

                foreach (var name in backing.Symbol.ContainingType.MemberNames)
                {
                    if (name.IsParts("Set", backing.Name))
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
