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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AttachedPropertyBrowsableForTypeArgumentFix))]
    [Shared]
    internal class AttachedPropertyBrowsableForTypeArgumentFix : DocumentEditorCodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            WPF0034AttachedPropertyBrowsableForTypeAttributeArgument.Descriptor.Id);

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

                if (syntaxRoot.FindNode(diagnostic.Location.SourceSpan).TryFirstAncestorOrSelf<AttributeArgumentSyntax>(out var argument) &&
                    argument.TryFirstAncestor<MethodDeclarationSyntax>(out var methodDeclaration) &&
                    semanticModel.TryGetSymbol(methodDeclaration, context.CancellationToken, out var method) &&
                    method.Parameters.TrySingle(out var parameter))
                {
                    context.RegisterCodeFix(
                        $"Change type to {parameter.Type}.",
                        (e, _) => Fix(e, argument, parameter.Type),
                        this.GetType(),
                        diagnostic);
                }
            }
        }

        private static void Fix(DocumentEditor editor, AttributeArgumentSyntax argument, ITypeSymbol returnType)
        {
            editor.ReplaceNode(
                argument.Expression,
                editor.Generator.TypeOfExpression(editor.Generator.TypeExpression(returnType)));
        }
    }
}
