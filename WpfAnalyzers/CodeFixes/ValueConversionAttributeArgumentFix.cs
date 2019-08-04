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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ValueConversionAttributeArgumentFix))]
    [Shared]
    internal class ValueConversionAttributeArgumentFix : DocumentEditorCodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(Descriptors.WPF0072ValueConversionMustUseCorrectTypes.Id);

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
                var attribute = argument.FirstAncestor<AttributeSyntax>();
                if (ValueConverter.TryGetConversionTypes(
                    attribute.FirstAncestor<ClassDeclarationSyntax>(),
                    semanticModel,
                    context.CancellationToken,
                    out var sourceType,
                    out var targetType))
                {
                    context.RegisterCodeFix(
                        $"Change to [ValueConversion(typeof({sourceType}), typeof({targetType}))].",
                        (e, _) => FixArgument(e, attribute, sourceType, targetType),
                        $"Change to [ValueConversion(typeof({sourceType}), typeof({targetType}))].",
                        diagnostic);
                }
            }
        }

        private static void FixArgument(DocumentEditor editor, AttributeSyntax attributeSyntax, ITypeSymbol inType, ITypeSymbol outType)
        {
            editor.ReplaceNode(
                attributeSyntax.ArgumentList.Arguments[0],
                editor.Generator.AttributeArgument(
                    editor.Generator.TypeOfExpression(editor.Generator.TypeExpression(inType))));

            editor.ReplaceNode(
                attributeSyntax.ArgumentList.Arguments[1],
                editor.Generator.AttributeArgument(
                    editor.Generator.TypeOfExpression(editor.Generator.TypeExpression(outType))));
        }
    }
}
