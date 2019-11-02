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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AttachedPropertyBrowsableForTypeArgumentFix))]
    [Shared]
    internal class AttachedPropertyBrowsableForTypeArgumentFix : DocumentEditorCodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.WPF0034AttachedPropertyBrowsableForTypeAttributeArgument.Id);

        /// <inheritdoc/>
        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var document = context.Document;
            var syntaxRoot = await document.GetSyntaxRootAsync(context.CancellationToken)
                                           .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNode(diagnostic, out AttributeArgumentSyntax? argument) &&
                    argument.TryFirstAncestor<MethodDeclarationSyntax>(out var methodDeclaration) &&
                    methodDeclaration.ParameterList is { } parameterList &&
                    parameterList.Parameters.TrySingle(out var parameter))
                {
                    context.RegisterCodeFix(
                        $"Change type to {parameter.Type}.",
                        (editor, _) => editor.ReplaceNode(
                            argument.Expression,
                            editor.Generator.TypeOfExpression(parameter.Type)),
                        this.GetType(),
                        diagnostic);
                }
            }
        }
    }
}
