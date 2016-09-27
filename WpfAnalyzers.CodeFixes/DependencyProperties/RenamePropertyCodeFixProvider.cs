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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RenameFieldCodeFixProvider))]
    [Shared]
    internal class RenamePropertyCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(WA1203ClrPropertyNameMustMatchRegisteredName.DiagnosticId);

        /// <inheritdoc/>
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var syntaxRoot = await document.GetSyntaxRootAsync(context.CancellationToken)
                                           .ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                var token = syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start);
                if (string.IsNullOrEmpty(token.ValueText))
                {
                    continue;
                }

                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Rename property to match registered name.",
                        cancellationToken =>
                            GetTransformedDocumentAsync(
                                context.Document,
                                syntaxRoot,
                                token,
                                diagnostic,
                                cancellationToken),
                        this.GetType().Name),
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
            var declaration = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                        .FirstAncestorOrSelf<PropertyDeclarationSyntax>();

            FieldDeclarationSyntax dependencyProperty;
            if (declaration.TryGetDependencyPropertyFromGetter(out dependencyProperty))
            {
                var registeredName = dependencyProperty.DependencyPropertyRegisteredName();
                return RenameHelper.RenameSymbolAsync(document, syntaxRoot, token, registeredName, cancellationToken);
            }

            return RenameHelper.RenameSymbolAsync(document, syntaxRoot, token, declaration.Name(), cancellationToken);
        }
    }
}