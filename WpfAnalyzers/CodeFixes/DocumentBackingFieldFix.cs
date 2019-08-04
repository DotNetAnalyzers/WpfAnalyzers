namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DocumentBackingFieldFix))]
    [Shared]
    internal class DocumentBackingFieldFix : DocumentEditorCodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(Descriptors.WPF0060DocumentDependencyPropertyBackingMember.Id);

        /// <inheritdoc/>
        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
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

                if (syntaxRoot.TryFindNodeOrAncestor(diagnostic, out MemberDeclarationSyntax member) &&
                    semanticModel.TryGetSymbol(member, context.CancellationToken, out ISymbol symbol) &&
                    BackingFieldOrProperty.TryCreateForDependencyProperty(symbol, out var fieldOrProperty) &&
                    DependencyProperty.TryGetRegisteredName(fieldOrProperty, semanticModel, context.CancellationToken, out var name))
                {
                    context.RegisterCodeFix(
                        "Add standard documentation.",
                        (editor, _) => editor.ReplaceNode(member, x => x.WithDocumentationText($"/// <summary>Identifies the <see cref=\"{name}\"/> dependency property.</summary>")),
                        this.GetType(),
                        diagnostic);
                }
            }
        }
    }
}
