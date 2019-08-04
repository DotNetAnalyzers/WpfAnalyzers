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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RenameMemberFix))]
    [Shared]
    internal class RenameMemberFix : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.WPF0001BackingFieldShouldMatchRegisteredName.Id,
            Descriptors.WPF0002BackingFieldShouldMatchRegisteredName.Id,
            Descriptors.WPF0003ClrPropertyShouldMatchRegisteredName.Id,
            Descriptors.WPF0004ClrMethodShouldMatchRegisteredName.Id,
            Descriptors.WPF0005PropertyChangedCallbackShouldMatchRegisteredName.Id,
            Descriptors.WPF0006CoerceValueCallbackShouldMatchRegisteredName.Id,
            Descriptors.WPF0007ValidateValueCallbackCallbackShouldMatchRegisteredName.Id,
            WPF0090RegisterClassHandlerCallbackNameShouldMatchEvent.Descriptor.Id,
            WPF0091AddAndRemoveHandlerCallbackNameShouldMatchEvent.Descriptor.Id,
            WPF0100BackingFieldShouldMatchRegisteredName.Descriptor.Id,
            WPF0102EventDeclarationName.Descriptor.Id);

        public override FixAllProvider GetFixAllProvider() => null;

        /// <inheritdoc/>
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var syntaxRoot = await document.GetSyntaxRootAsync(context.CancellationToken)
                                           .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start) is var token &&
                    token.IsKind(SyntaxKind.IdentifierToken) &&
                    diagnostic.Properties.TryGetValue("ExpectedName", out var registeredName))
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            $"Rename to: '{registeredName}'.",
                            cancellationToken => ApplyFixAsync(context.Document, token, registeredName, cancellationToken),
                            this.GetType().FullName),
                        diagnostic);
                }
            }
        }

        private static async Task<Solution> ApplyFixAsync(Document document, SyntaxToken token, string newName, CancellationToken cancellationToken)
        {
            var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken)
                                           .ConfigureAwait(false);
            return await RenameHelper.RenameSymbolAsync(document, syntaxRoot, token, newName, cancellationToken)
                                     .ConfigureAwait(false);
        }
    }
}
