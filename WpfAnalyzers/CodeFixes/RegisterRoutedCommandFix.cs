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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RegisterRoutedCommandFix))]
    [Shared]
    internal class RegisterRoutedCommandFix : DocumentEditorCodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            WPF0122RegisterRoutedCommand.Descriptor.Id);

        /// <inheritdoc/>
        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNodeOrAncestor(diagnostic, out ArgumentListSyntax argumentList) &&
                    argumentList.Parent is ObjectCreationExpressionSyntax objectCreation &&
                    diagnostic.Properties.TryGetValue(nameof(IdentifierNameSyntax), out var name) &&
                    diagnostic.Properties.TryGetValue(nameof(TypeOfExpressionSyntax), out var type))
                {
                    if (objectCreation.Type == KnownSymbol.RoutedCommand)
                    {
                        context.RegisterCodeFix(
                            "Register with containing type and containing member.",
                            (e, _) => e.ReplaceNode(
                                argumentList,
                                x => x.AddArguments(
                                    ParseArgument($"nameof({name})"),
                                    ParseArgument($"typeof({type})"))),
                            this.GetType(),
                            diagnostic);
                    }

                    if (objectCreation.Type == KnownSymbol.RoutedUICommand)
                    {
                        context.RegisterCodeFix(
                            "Register with containing type and containing member.",
                            (e, _) => e.ReplaceNode(
                                argumentList,
                                x => x.AddArguments(
                                    ParseArgument($"\"PLACEHOLDER TEXT\""),
                                    ParseArgument($"nameof({name})"),
                                    ParseArgument($"typeof({type})"))),
                            this.GetType(),
                            diagnostic);
                    }
                }
            }
        }

        private static ArgumentSyntax ParseArgument(string expression)
        {
            return SyntaxFactory.Argument(SyntaxFactory.ParseExpression(expression));
        }
    }
}
