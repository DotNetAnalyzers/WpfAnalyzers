namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FixCastCodeFixProvider))]
    [Shared]
    internal class FixCastCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            WPF0019CastSenderToCorrectType.DiagnosticId);

        public override FixAllProvider GetFixAllProvider() => DocumentEditorFixAllProvider.Default;

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

                if (diagnostic.Properties.TryGetValue("ExpectedType", out var registeredType))
                {
                    if (syntaxRoot.FindNode(diagnostic.Location.SourceSpan).FirstAncestorOrSelf<IdentifierNameSyntax>() is IdentifierNameSyntax identifierName &&
                        !identifierName.IsMissing)
                    {
                        context.RegisterDocumentEditorFix(
                            $"Change type to: {registeredType}.",
                            (e, _) => ChangeType(e, identifierName, registeredType),
                            this.GetType().FullName,
                            diagnostic);
                    }
                }
            }
        }

        private static void ChangeType(DocumentEditor editor, SimpleNameSyntax typeSyntax, string newType)
        {
            editor.ReplaceNode(
                typeSyntax,
                (x, _) => ((SimpleNameSyntax)x).WithIdentifier(SyntaxFactory.ParseToken(newType)));
        }

        private static void ChangeType(DocumentEditor editor, IdentifierNameSyntax typeSyntax, string newType)
        {
            editor.ReplaceNode(
                typeSyntax,
                (x, _) => ((IdentifierNameSyntax)x).WithIdentifier(SyntaxFactory.ParseToken(newType)));
        }
    }
}