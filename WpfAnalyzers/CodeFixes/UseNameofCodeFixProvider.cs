namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseNameofCodeFixProvider))]
    [Shared]
    internal class UseNameofCodeFixProvider : DocumentEditorCodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            WPF0120RegisterContainingMemberAsNameForRoutedCommand.DiagnosticId);

        /// <inheritdoc/>
        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var document = context.Document;
            var syntaxRoot = await document.GetSyntaxRootAsync(context.CancellationToken)
                                           .ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                if (diagnostic.Properties.TryGetValue(nameof(IdentifierNameSyntax), out var name) &&
                    syntaxRoot.TryFindNodeOrAncestor(diagnostic, out ArgumentSyntax argument))
                {
                    context.RegisterCodeFix(
                        $"Use nameof({name}).",
                        (editor, _) => editor.ReplaceNode(
                            argument.Expression,
                            x => SyntaxFactory.ParseExpression($"nameof({name})")),
                        "Use containing type.",
                        diagnostic);
                }
            }
        }
    }
}
