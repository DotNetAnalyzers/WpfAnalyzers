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
    using Microsoft.CodeAnalysis.Editing;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ConstructorArgumentAttributeArgumentFix))]
    [Shared]
    internal class ConstructorArgumentAttributeArgumentFix : DocumentEditorCodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            WPF0082ConstructorArgument.DiagnosticId);

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

                var argument = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                         .FirstAncestorOrSelf<AttributeArgumentSyntax>();
                var attribute = argument.FirstAncestor<AttributeSyntax>();
                if (ConstructorArgument.IsMatch(attribute, out var arg, out var parameterName) == false)
                {
                    context.RegisterCodeFix(
                        $"Change to [ConstructorArgument(\"{parameterName})\"))].",
                        (e, _) => FixArgument(e, arg, parameterName),
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
