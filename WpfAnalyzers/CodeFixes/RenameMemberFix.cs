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
            Descriptors.WPF0090RegisterClassHandlerCallbackNameShouldMatchEvent.Id,
            Descriptors.WPF0091AddAndRemoveHandlerCallbackNameShouldMatchEvent.Id,
            Descriptors.WPF0100BackingFieldShouldMatchRegisteredName.Id,
            Descriptors.WPF0102EventDeclarationName.Id);

        public override FixAllProvider? GetFixAllProvider() => null;

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
                    diagnostic.Properties.TryGetValue("ExpectedName", out var newName))
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            $"Rename to: '{newName}'.",
                            cancellationToken => RenameHelper.RenameSymbolAsync(document, syntaxRoot, token, newName, cancellationToken),
                            this.GetType().FullName),
                        diagnostic);
                }
            }
        }
    }
}
