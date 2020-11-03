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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ComponentResourceKeyFix))]
    [Shared]
    internal class ComponentResourceKeyFix : DocumentEditorCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.WPF0140UseContainingTypeComponentResourceKey.Id,
            Descriptors.WPF0141UseContainingMemberComponentResourceKey.Id);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var document = context.Document;
            var syntaxRoot = await document.GetSyntaxRootAsync(context.CancellationToken)
                                           .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot is { } &&
                    diagnostic.Id == Descriptors.WPF0140UseContainingTypeComponentResourceKey.Id &&
                    syntaxRoot.TryFindNodeOrAncestor(diagnostic, out ObjectCreationExpressionSyntax? objectCreation) &&
                    objectCreation.ArgumentList is { Arguments: { Count: 0 } } argumentList &&
                    diagnostic.Properties.TryGetValue(nameof(ArgumentListSyntax), out var argumentListString))
                {
                    context.RegisterCodeFix(
                        "Use default arguments for ComponentResourceKey.",
                        (editor, _) => editor.ReplaceNode(
                            argumentList,
                            x => SyntaxFactory.ParseArgumentList($"({argumentListString})")),
                        "Use default arguments for ComponentResourceKey.",
                        diagnostic);
                }

                if (diagnostic.Id == Descriptors.WPF0141UseContainingMemberComponentResourceKey.Id &&
                    syntaxRoot.TryFindNodeOrAncestor(diagnostic, out ArgumentSyntax? argument) &&
                    diagnostic.Properties.TryGetValue(nameof(ArgumentSyntax), out var argumentString))
                {
                    context.RegisterCodeFix(
                        "Use default key.",
                        (editor, _) => editor.ReplaceNode(
                            argument.Expression,
                            x => SyntaxFactory.ParseExpression(argumentString).WithTriviaFrom(x)),
                        "Use default key.",
                        diagnostic);
                }
            }
        }
    }
}
