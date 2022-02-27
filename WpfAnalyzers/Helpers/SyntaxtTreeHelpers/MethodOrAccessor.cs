namespace WpfAnalyzers;

using Microsoft.CodeAnalysis.CSharp.Syntax;

internal readonly struct MethodOrAccessor
{
    internal readonly ArrowExpressionClauseSyntax? ExpressionBody;
    internal readonly BlockSyntax? Body;

    internal MethodOrAccessor(ArrowExpressionClauseSyntax? expressionBody, BlockSyntax? body)
    {
        this.ExpressionBody = expressionBody;
        this.Body = body;
    }

    internal static MethodOrAccessor Create(AccessorDeclarationSyntax accessor) => new(accessor.ExpressionBody, accessor.Body);

    internal static MethodOrAccessor Create(MethodDeclarationSyntax accessor) => new(accessor.ExpressionBody, accessor.Body);
}