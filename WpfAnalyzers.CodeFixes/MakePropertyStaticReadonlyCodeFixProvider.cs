namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MakePropertyStaticReadonlyCodeFixProvider))]
    [Shared]
    internal class MakePropertyStaticReadonlyCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(WPF0030BackingFieldShouldBeStaticReadonly.DiagnosticId);

        /// <inheritdoc/>
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                var token = syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start);
                if (string.IsNullOrEmpty(token.ValueText))
                {
                    continue;
                }

                var declaration = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                                 .FirstAncestorOrSelf<PropertyDeclarationSyntax>();
                if (declaration == null ||
                    declaration.IsMissing)
                {
                    continue;
                }

                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Make static readonly",
                        cancellationToken => ApplyFixAsync(context.Document, declaration, cancellationToken),
                        this.GetType().FullName),
                    diagnostic);
            }
        }

        private static async Task<Document> ApplyFixAsync(Document document, PropertyDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken)
                                             .ConfigureAwait(false);

            editor.ReplaceNode(declaration, declaration.WithModifiers(declaration.Modifiers.WithStatic()));

            if (declaration.TryGetSetAccessorDeclaration(out var setter) &&
                setter.Body == null)
            {
                editor.RemoveNode(setter, SyntaxRemoveOptions.KeepLeadingTrivia);
            }

            if (declaration.ExpressionBody != null)
            {
                editor.ReplaceNode(
                    declaration,
                    (x, g) => ((PropertyDeclarationSyntax)x)
                        .WithInitializer(SyntaxFactory.EqualsValueClause(declaration.ExpressionBody.Expression))
                        .WithExpressionBody(null)
                        .WithAccessorList(
                            SyntaxFactory.AccessorList(
                                SyntaxFactory.SingletonList(
                                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                                 .WithSemicolonToken(
                                                     SyntaxFactory.Token(SyntaxKind.SemicolonToken))))));
            }

            return editor.GetChangedDocument();
        }
    }
}