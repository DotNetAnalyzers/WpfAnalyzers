namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MakeFieldStaticReadonlyCodeFixProvider))]
    [Shared]
    internal class MakeFieldStaticReadonlyCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(WA1202FieldMustBeStaticReadOnly.DiagnosticId);

        /// <inheritdoc/>
        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Make static readonly",
                        cancellationToken => GetTransformedDocumentAsync(context.Document, diagnostic, cancellationToken),
                        nameof(MakeFieldStaticReadonlyCodeFixProvider)),
                    diagnostic);
            }

            return FinishedTasks.CompletedTask;
        }


        private static async Task<Document> GetTransformedDocumentAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var fieldDeclaration = syntaxRoot.FindNode(diagnostic.Location.SourceSpan).FirstAncestorOrSelf<FieldDeclarationSyntax>();
            if (fieldDeclaration == null || fieldDeclaration.IsMissing)
            {
                return document;
            }

            var updatedModifiers = fieldDeclaration.Modifiers
                                                   .WithStatic()
                                                   .WithReadOnly();
            var updatedDeclaration = fieldDeclaration.WithModifiers(updatedModifiers);
            syntaxRoot = syntaxRoot.ReplaceNode(fieldDeclaration, updatedDeclaration);
            return document.WithSyntaxRoot(syntaxRoot);
        }
    }
}
