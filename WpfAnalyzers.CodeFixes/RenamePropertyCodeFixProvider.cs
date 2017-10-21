namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;

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
            var semanticModel = await document.GetSemanticModelAsync(context.CancellationToken)
                                              .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                var token = syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start);
                if (string.IsNullOrEmpty(token.ValueText))
                {
                    continue;
                }

                var node = syntaxRoot.FindNode(diagnostic.Location.SourceSpan);
                if (node == null ||
                    node.IsMissing)
                {
                    continue;
                }

                var property = semanticModel.SemanticModelFor(node)
                                            .GetDeclaredSymbol(node, context.CancellationToken) as IPropertySymbol;
                if (ClrProperty.TryGetRegisteredName(
                    property,
                    semanticModel,
                    context.CancellationToken,
                    out string registeredName))
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            $"Rename to: {registeredName}.",
                            _ => ApplyFixAsync(context, token, registeredName),
                            this.GetType()
                                .FullName),
                        diagnostic);
                }
            }
        }

        private static async Task<Solution> ApplyFixAsync(CodeFixContext context, SyntaxToken token, string newName)
        {
            var document = context.Document;
            var syntaxRoot = await document.GetSyntaxRootAsync(context.CancellationToken)
                                           .ConfigureAwait(false);
            return await RenameHelper.RenameSymbolAsync(document, syntaxRoot, token, newName, context.CancellationToken)
                                     .ConfigureAwait(false);
        }
    }
}