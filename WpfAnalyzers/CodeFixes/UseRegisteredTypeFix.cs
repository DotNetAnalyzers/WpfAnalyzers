namespace WpfAnalyzers;

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;

using Gu.Roslyn.AnalyzerExtensions;
using Gu.Roslyn.CodeFixExtensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseRegisteredTypeFix))]
[Shared]
internal class UseRegisteredTypeFix : DocumentEditorCodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
        Descriptors.WPF0012ClrPropertyShouldMatchRegisteredType.Id,
        Descriptors.WPF0013ClrMethodMustMatchRegisteredType.Id);

    protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
    {
        var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                      .ConfigureAwait(false);
        foreach (var diagnostic in context.Diagnostics)
        {
            if (syntaxRoot is { } &&
                diagnostic.Properties.TryGetValue(nameof(TypeSyntax), out var typeText) &&
                typeText is { } &&
                syntaxRoot.TryFindNode(diagnostic, out TypeSyntax? typeSyntax))
            {
                if (typeSyntax.Parent is PropertyDeclarationSyntax property &&
                    property.Getter() is { } getter)
                {
                    context.RegisterCodeFix(
                        $"Change to: {typeText}.",
                        (editor, _) => editor.ReplaceNode(
                                                 typeSyntax,
                                                 x => SyntaxFactory.ParseTypeName(typeText).WithTriviaFrom(x))
                                             .ReplaceNode(
                                                 getter,
                                                 x => CastReturnValue.Rewrite(x, typeText)),
                        nameof(UseRegisteredTypeFix),
                        diagnostic);
                }
                else if (typeSyntax.Parent is MethodDeclarationSyntax { ParameterList: { Parameters: { Count: 1 } } } method &&
                         method.Identifier.ValueText.StartsWith("Get", StringComparison.Ordinal))
                {
                    context.RegisterCodeFix(
                        $"Change to: {typeText}.",
                        (editor, _) => editor.ReplaceNode(
                                                 typeSyntax,
                                                 x => SyntaxFactory.ParseTypeName(typeText).WithTriviaFrom(x))
                                             .ReplaceNode(
                                                 method,
                                                 x => CastReturnValue.Rewrite(x, typeText)),
                        nameof(UseRegisteredTypeFix),
                        diagnostic);
                }
                else
                {
                    context.RegisterCodeFix(
                        $"Change to: {typeText}.",
                        (editor, _) => editor.ReplaceNode(
                            typeSyntax,
                            x => SyntaxFactory.ParseTypeName(typeText).WithTriviaFrom(x)),
                        nameof(UseRegisteredTypeFix),
                        diagnostic);
                }
            }
        }
    }

    private class CastReturnValue : CSharpSyntaxRewriter
    {
        private readonly string typeText;

        private CastReturnValue(string typeText)
        {
            this.typeText = typeText;
        }

        public override SyntaxNode? VisitArrowExpressionClause(ArrowExpressionClauseSyntax node)
        {
            return node switch
            {
                { Expression: CastExpressionSyntax { Type: { } type } } => node.ReplaceNode(type, SyntaxFactory.ParseTypeName(this.typeText).WithTriviaFrom(type)),
                { Expression: { } expression } => SyntaxFactory.CastExpression(SyntaxFactory.ParseTypeName(this.typeText), expression),
                _ => base.VisitArrowExpressionClause(node),
            };
        }

        public override SyntaxNode? VisitReturnStatement(ReturnStatementSyntax node)
        {
            return node switch
            {
                { Expression: CastExpressionSyntax { Type: { } type } } => node.ReplaceNode(type, SyntaxFactory.ParseTypeName(this.typeText).WithTriviaFrom(type)),
                { Expression: { } expression } => SyntaxFactory.CastExpression(SyntaxFactory.ParseTypeName(this.typeText), expression),
                _ => base.VisitReturnStatement(node),
            };
        }

        internal static T Rewrite<T>(T node, string typeText)
            where T : CSharpSyntaxNode
        {
            return (T)new CastReturnValue(typeText).Visit(node);
        }
    }
}
