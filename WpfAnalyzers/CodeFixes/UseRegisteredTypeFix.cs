namespace WpfAnalyzers
{
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
            WPF0012ClrPropertyShouldMatchRegisteredType.DiagnosticId);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (diagnostic.Properties.TryGetValue(nameof(TypeSyntax), out var typeText) &&
                    syntaxRoot.TryFindNode(diagnostic, out TypeSyntax typeSyntax) &&
                    typeSyntax.Parent is PropertyDeclarationSyntax property &&
                    property.TryGetGetter(out var getter))
                {
                    context.RegisterCodeFix(
                        $"Change to: {typeText}.",
                        (editor, _) => editor.ReplaceNode(
                                                 typeSyntax,
                                                 x => SyntaxFactory.ParseTypeName(typeText).WithTriviaFrom(x))
                                             .ReplaceNode(
                                                 getter,
                                                 x => WithCast(x, typeText)),
                        nameof(UseRegisteredTypeFix),
                        diagnostic);
                }
            }
        }

        private static AccessorDeclarationSyntax WithCast(AccessorDeclarationSyntax getter, string typeText)
        {
            switch (getter.ExpressionBody)
            {
                case ArrowExpressionClauseSyntax arrow when arrow.Expression is CastExpressionSyntax cast:
                    return getter.ReplaceNode(cast.Type, SyntaxFactory.ParseTypeName(typeText).WithTriviaFrom(cast.Type));
                case ArrowExpressionClauseSyntax arrow:
                    return getter.ReplaceNode(arrow.Expression, SyntaxFactory.CastExpression(SyntaxFactory.ParseTypeName(typeText), arrow.Expression));
            }

            if (getter.Body is BlockSyntax block &&
                block.Statements.TrySingleOfType(out ReturnStatementSyntax statement))
            {
                if (statement.Expression is CastExpressionSyntax cast)
                {
                    return getter.ReplaceNode(cast.Type, SyntaxFactory.ParseTypeName(typeText).WithTriviaFrom(cast.Type));
                }

                return getter.ReplaceNode(statement.Expression, SyntaxFactory.CastExpression(SyntaxFactory.ParseTypeName(typeText), statement.Expression));
            }

            return getter;
        }
    }
}
