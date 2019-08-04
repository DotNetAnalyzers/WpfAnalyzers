namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ConstructorArgumentAttributeArgumentFix))]
    [Shared]
    internal class ConstructorArgumentAttributeArgumentFix : DocumentEditorCodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.WPF0082ConstructorArgument.Id);

        /// <inheritdoc/>
        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var document = context.Document;
            var syntaxRoot = await document.GetSyntaxRootAsync(context.CancellationToken)
                                           .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNodeOrAncestor<AttributeArgumentSyntax>(diagnostic, out var argument) &&
                    diagnostic.Properties.TryGetValue(nameof(ConstructorArgument), out var parameterName))
                {
                    context.RegisterCodeFix(
                        $"Change to [ConstructorArgument(\"{parameterName})\"))].",
                        (e, _) => FixArgument(e, argument, parameterName),
                        this.GetType(),
                        diagnostic);
                }
            }
        }

        private static void FixArgument(DocumentEditor editor, AttributeArgumentSyntax argument, string parameterName)
        {
            editor.ReplaceNode(
                argument.Expression,
                (_, generator) => generator.LiteralExpression(parameterName));
        }
    }
}
