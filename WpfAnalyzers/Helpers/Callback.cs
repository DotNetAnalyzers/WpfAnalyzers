namespace WpfAnalyzers;

using System.Threading;

using Gu.Roslyn.AnalyzerExtensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

internal readonly struct Callback
{
    internal readonly IdentifierNameSyntax Identifier;
    internal readonly IMethodSymbol Target;

    internal Callback(IdentifierNameSyntax identifier, IMethodSymbol target)
    {
        this.Identifier = identifier;
        this.Target = target;
    }

    internal static Callback? Match(ArgumentSyntax callback, QualifiedType handlerType, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        return callback.Expression switch
        {
            IdentifierNameSyntax identifierName
                when FindMethod(identifierName) is { } method
                => new Callback(identifierName, method),
            MemberAccessExpressionSyntax { Name: IdentifierNameSyntax identifierName }
                    memberAccess
                when memberAccess.IsKind(SyntaxKind.SimpleMemberAccessExpression) &&
                     FindMethod(identifierName) is { } method
                => new Callback(identifierName, method),
            LambdaExpressionSyntax { Body: InvocationExpressionSyntax invocation } =>
                invocation switch
                {
                    { Expression: IdentifierNameSyntax identifierName }
                        when FindMethod(identifierName) is { } method
                        => new Callback(identifierName, method),
                    { Expression: MemberAccessExpressionSyntax { Name: IdentifierNameSyntax identifierName } }
                        when FindMethod(identifierName) is { } method
                        => new Callback(identifierName, method),
                    _ => null,
                },

            ObjectCreationExpressionSyntax { ArgumentList: { Arguments: { Count: 1 } arguments } } creation
                when creation.IsType(handlerType, semanticModel, cancellationToken)
                => Match(arguments[0], handlerType, semanticModel, cancellationToken),
            _ => null,
        };

        IMethodSymbol? FindMethod(IdentifierNameSyntax candidate)
        {
            if (semanticModel.TryGetSymbol(candidate, cancellationToken, out var symbol) &&
                symbol is IMethodSymbol method)
            {
                return method;
            }

            return null;
        }
    }

    internal static InvocationExpressionSyntax? SingleInvocation(IMethodSymbol method, SyntaxNode? scope, SyntaxNodeAnalysisContext context)
    {
        if (scope is null)
        {
            return null;
        }

        InvocationExpressionSyntax? invocation = null;
        using var walker = SpecificIdentifierNameWalker.Borrow(scope, method.Name);
        foreach (var identifierName in walker.IdentifierNames)
        {
            if (identifierName.Parent is MemberAccessExpressionSyntax { Parent: InvocationExpressionSyntax candidate } &&
                context.SemanticModel.TryGetSymbol(identifierName, context.CancellationToken, out IMethodSymbol? symbol) &&
                MethodSymbolComparer.Equal(symbol, method))
            {
                if (invocation is { })
                {
                    return null;
                }

                invocation = candidate;
            }
        }

        return invocation;
    }

    internal static bool CanInlineBody(MethodDeclarationSyntax method)
    {
        if (method.Modifiers.Any(SyntaxKind.ProtectedKeyword, SyntaxKind.InternalKeyword, SyntaxKind.PublicKeyword))
        {
            return false;
        }

        return method switch
        {
            { ExpressionBody: { } } => true,
            { Body: { Statements: { Count: 1 } statements } } => statements[0].IsEither(SyntaxKind.ExpressionStatement, SyntaxKind.ReturnStatement),
            { Body: { Statements: { Count: 2 } statements } }
                when statements[0] is LocalDeclarationStatementSyntax { Declaration: { Variables: { Count: 1 } variables } } &&
                     variables[0].Initializer is { Value: CastExpressionSyntax _ }
                => statements[1].IsEither(SyntaxKind.ExpressionStatement, SyntaxKind.ReturnStatement),
            _ => false,
        };
    }

    internal static bool IsInvokedOnce(IMethodSymbol method, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        using var walker = InvocationWalker.InContainingClass(method, semanticModel, cancellationToken);
        return walker.IdentifierNames.Count == 1;
    }
}
