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
            WPF0034AttachedPropertyBrowsableForTypeAttributeArgument.DiagnosticId);

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

                var argument = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                         .FirstAncestorOrSelf<AttributeArgumentSyntax>();
                var methodDeclaration = argument.FirstAncestor<AttributeSyntax>()
                                        .FirstAncestor<MethodDeclarationSyntax>();
                if (AttachedPropertyBrowsableForType.TryGetParameterType(methodDeclaration, semanticModel, context.CancellationToken, out var parameterType))
                {
                    context.RegisterCodeFix(
                        $"Change type to {parameterType}.",
                        (e, _) => Fix(e, argument, parameterType),
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
