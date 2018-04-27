namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DocumentBackingFieldCodeFixProvider))]
    [Shared]
    internal class DocumentBackingFieldCodeFixProvider : DocumentEditorCodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(WPF0060DocumentDependencyPropertyBackingField.DiagnosticId);

        /// <inheritdoc/>
        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
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
                        "Add xml documentation.",
                        (editor, cancellationToken) => AddDocumentation(editor, member, cancellationToken),
                        this.GetType(),
                        diagnostic);
                }
            }
        }

        private static void AddDocumentation(DocumentEditor editor, MemberDeclarationSyntax member, CancellationToken cancellationToken)
        {
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
        }
    }
}
