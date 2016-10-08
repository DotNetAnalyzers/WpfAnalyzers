namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RenamePropertyCodeFixProvider))]
    [Shared]
    internal class RenamePropertyCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(WPF0003ClrPropertyShouldMatchRegisteredName.DiagnosticId);

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
                        _ => ApplyFixAsync(context, diagnostic),
                        this.GetType().Name),
                    diagnostic);
            }
        }

        private static async Task<Solution> ApplyFixAsync(CodeFixContext context, Diagnostic diagnostic)
        {
            var document = context.Document;
            var syntaxRoot = await document.GetSyntaxRootAsync(context.CancellationToken)
                                           .ConfigureAwait(false);

            var semanticModel = await document.GetSemanticModelAsync(context.CancellationToken)
                                              .ConfigureAwait(false);

            var token = syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start);
            var declaration = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                        .FirstAncestorOrSelf<PropertyDeclarationSyntax>();

            string registeredName;
            if (declaration.TryGetDependencyPropertyRegisteredName(semanticModel, context.CancellationToken, out registeredName))
            {
                return await RenameHelper.RenameSymbolAsync(document, syntaxRoot, token, registeredName, context.CancellationToken).ConfigureAwait(false);
            }

            return document.Project.Solution;
        }
    }
}