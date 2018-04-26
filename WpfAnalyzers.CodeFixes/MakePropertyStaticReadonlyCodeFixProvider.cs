namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MakePropertyStaticReadonlyCodeFixProvider))]
    [Shared]
    internal class MakePropertyStaticReadonlyCodeFixProvider : DocumentEditorCodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(WPF0030BackingFieldShouldBeStaticReadonly.DiagnosticId);

        /// <inheritdoc/>
        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
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
                    "Make static readonly",
                    (e, _) => ApplyFix(e, declaration),
                    this.GetType(),
                    diagnostic);
            }
        }

        private static void ApplyFix(DocumentEditor editor, PropertyDeclarationSyntax declaration)
        {
            editor.ReplaceNode(declaration, declaration.WithModifiers(declaration.Modifiers.WithStatic()));

            if (declaration.TryGetSetter(out var setter) &&
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
        }
    }
}
