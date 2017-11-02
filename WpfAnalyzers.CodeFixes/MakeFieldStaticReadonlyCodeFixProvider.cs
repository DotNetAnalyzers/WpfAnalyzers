namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MakeFieldStaticReadonlyCodeFixProvider))]
    [Shared]
    internal class MakeFieldStaticReadonlyCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(WPF0030BackingFieldShouldBeStaticReadonly.DiagnosticId);

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider() => DocumentEditorFixAllProvider.Default;

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

                var fieldDeclaration = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                     .FirstAncestorOrSelf<FieldDeclarationSyntax>();
                if (fieldDeclaration == null || fieldDeclaration.IsMissing)
                {
                    continue;
                }

                context.RegisterDocumentEditorFix(
                        "Make static readonly",
                        (e, _) => ApplyFix(e, fieldDeclaration),
                        this.GetType(),
                    diagnostic);
            }
        }

        private static void ApplyFix(DocumentEditor context, FieldDeclarationSyntax fieldDeclaration)
        {
            context.ReplaceNode(fieldDeclaration, (x, _) =>
            {
                var f = (FieldDeclarationSyntax)x;
                return f.WithModifiers(
                    f.Modifiers
                     .WithStatic()
                     .WithReadOnly());
            });
        }
    }
}
