namespace WpfAnalyzers.DependencyProperties
{
    using System;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseDependencyPropertyKeyCodeFixProvider))]
    [Shared]
    internal class UseDependencyPropertyKeyCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(WA1220UseDependencyPropertyKeyForSettingReadOnlyProperties.DiagnosticId);

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
                        "Use DependencyPropertyKey when setting a readonly property.",
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
            throw new NotImplementedException();
        }
    }
}