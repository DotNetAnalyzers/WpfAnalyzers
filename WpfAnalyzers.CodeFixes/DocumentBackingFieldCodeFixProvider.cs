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
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DocumentBackingFieldCodeFixProvider))]
    [Shared]
    internal class DocumentBackingFieldCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(WPF0060DocumentBackingField.DiagnosticId);

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

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

                var member = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                       .FirstAncestorOrSelf<MemberDeclarationSyntax>();
                if (member != null)
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            "Add xml documentation.",
                            cancellationToken => AddDocumentationAsync(context.Document, member, cancellationToken),
                            this.GetType().FullName),
                        diagnostic);
                }
            }
        }

        private static async Task<Document> AddDocumentationAsync(Document document, MemberDeclarationSyntax member, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            var symbol = editor.SemanticModel.GetDeclaredSymbolSafe(member, cancellationToken);
            if (BackingFieldOrProperty.TryCreate(symbol, out var fieldOrProperty) &&
                DependencyProperty.TryGetRegisteredName(fieldOrProperty, editor.SemanticModel, cancellationToken, out var name))
            {
                editor.ReplaceNode(
                    member,
                    member.WithLeadingTrivia(
                        member.GetLeadingTrivia()
                              .AddRange(SyntaxFactory.ParseLeadingTrivia($"/// <summary>Identifies the <see cref=\"{name}\"/> dependency property.</summary>"))
                              .Add(SyntaxFactory.ElasticLineFeed)));
            }

            return editor.GetChangedDocument();
        }
    }
}