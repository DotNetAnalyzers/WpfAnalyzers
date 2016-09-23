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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RenameDependencyPropertyFieldCodeFixProvider))]
    [Shared]
    internal class RenameDependencyPropertyFieldCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(WA1200DependencyPropertyFieldMustBeNamedNameProperty.DiagnosticId);

        /// <inheritdoc/>
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var syntaxRoot = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                var token = syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start);
                if (string.IsNullOrEmpty(token.ValueText))
                {
                    continue;
                }

                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Rename field to match registered name.",
                        cancellationToken => GetTransformedDocumentAsync(context.Document, syntaxRoot, token, diagnostic, cancellationToken),
                        nameof(MakeFieldStaticReadonlyCodeFixProvider)),
                    diagnostic);
            }
        }

        private static Task<Solution> GetTransformedDocumentAsync(
            Document document,
            SyntaxNode syntaxRoot,
            SyntaxToken token,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var fieldDeclaration = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                             .FirstAncestorOrSelf<FieldDeclarationSyntax>();
            var registeredName = fieldDeclaration.RegisteredDependencyPropertyName();
            var updatedDeclaration = RenameHelper.RenameSymbolAsync(
                document,
                syntaxRoot,
                token,
                registeredName + "Property",
                cancellationToken);
            return updatedDeclaration;
        }
    }
}
