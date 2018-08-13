namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DocumentClrMethodCodeFixProvider))]
    [Shared]
    internal class DocumentOnPropertyChangedCodeFixProvider : DocumentEditorCodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            WPF0062DocumentPropertyChangedCallback.DiagnosticId);

        /// <inheritdoc/>
        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var document = context.Document;
            var syntaxRoot = await document.GetSyntaxRootAsync(context.CancellationToken)
                                           .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNodeOrAncestor(diagnostic, out MethodDeclarationSyntax methodDeclaration) &&
                    diagnostic.Properties.TryGetValue(nameof(WPF0062DocumentPropertyChangedCallback), out var text) &&
                    text != null)
                {
                    context.RegisterCodeFix(
                        "Add standard documentation.",
                        (editor, _) => editor.ReplaceNode(methodDeclaration, x => x.WithDocumentationText(text)),
                        this.GetType(),
                        diagnostic);
                }
            }
        }
    }
}
