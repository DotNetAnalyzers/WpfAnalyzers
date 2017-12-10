namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RenameMethodCodeFixProvider))]
    [Shared]
    internal class RenameMethodCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(
                WPF0004ClrMethodShouldMatchRegisteredName.DiagnosticId,
                WPF0005PropertyChangedCallbackShouldMatchRegisteredName.DiagnosticId,
                WPF0006CoerceValueCallbackShouldMatchRegisteredName.DiagnosticId,
                WPF0007ValidateValueCallbackCallbackShouldMatchRegisteredName.DiagnosticId,
                WPF0090RegisterClassHandlerCallbackNameShouldMatchEvent.DiagnosticId,
                WPF0091AddAndRemoveHandlerCallbackNameShouldMatchEvent.DiagnosticId);

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

                if (diagnostic.Properties.TryGetValue("ExpectedName", out var newName))
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            $"Rename to: {newName}",
                            cancellationToken => RenameHelper.RenameSymbolAsync(
                                context.Document,
                                syntaxRoot,
                                token,
                                newName,
                                cancellationToken),
                            this.GetType().FullName),
                        diagnostic);
                }
            }
        }
    }
}