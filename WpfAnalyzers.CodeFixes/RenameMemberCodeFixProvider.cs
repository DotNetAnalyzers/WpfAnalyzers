namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RenameMemberCodeFixProvider))]
    [Shared]
    internal class RenameMemberCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            WPF0003ClrPropertyShouldMatchRegisteredName.DiagnosticId,
            WPF0004ClrMethodShouldMatchRegisteredName.DiagnosticId,
            WPF0005PropertyChangedCallbackShouldMatchRegisteredName.DiagnosticId,
            WPF0006CoerceValueCallbackShouldMatchRegisteredName.DiagnosticId,
            WPF0007ValidateValueCallbackCallbackShouldMatchRegisteredName.DiagnosticId,
            WPF0090RegisterClassHandlerCallbackNameShouldMatchEvent.DiagnosticId,
            WPF0091AddAndRemoveHandlerCallbackNameShouldMatchEvent.DiagnosticId,
            WPF0102EventDeclarationName.DiagnosticId);

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

                var memberDeclaration = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                                  .FirstAncestorOrSelf<MemberDeclarationSyntax>();
                if (memberDeclaration == null ||
                    memberDeclaration.IsMissing)
                {
                    continue;
                }

                if (diagnostic.Properties.TryGetValue("ExpectedName", out var registeredName))
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            $"Rename to: {registeredName}.",
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