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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ChangeTypeofFix))]
    [Shared]
    internal class ChangeTypeofFix : DocumentEditorCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.WPF0081MarkupExtensionReturnTypeMustUseCorrectType.Id);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var document = context.Document;
            var syntaxRoot = await document.GetSyntaxRootAsync(context.CancellationToken)
                                           .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot is { } &&
                    syntaxRoot.TryFindNode(diagnostic, out ExpressionSyntax? expression) &&
                    diagnostic.Properties.TryGetValue(nameof(ITypeSymbol), out var returnType) &&
                    returnType is { })
                {
                    context.RegisterCodeFix(
                        $"Change type to {returnType}.",
                        (e, _) => e.ReplaceNode(
                            expression,
                            x => SyntaxFactory.ParseExpression(returnType).WithTriviaFrom(x)),
                        this.GetType(),
                        diagnostic);
                }
            }
        }
    }
}
