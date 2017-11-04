namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddValueConversionAttributeFix))]
    [Shared]
    internal class AddValueConversionAttributeFix : CodeFixProvider
    {
        private static readonly AttributeSyntax Attribute = SyntaxFactory
            .Attribute(SyntaxFactory.ParseName("System.Windows.Data.ValueConversionAttribute"))
            .WithSimplifiedNames();

        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(WPF0071ConverterDoesNotHaveAttribute.DiagnosticId);

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        /// <inheritdoc/>
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
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

                var classDeclaration = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                                       .FirstAncestorOrSelf<ClassDeclarationSyntax>();
                if (classDeclaration != null &&
                    ValueConverter.TryGetConversionTypes(classDeclaration, semanticModel, context.CancellationToken, out var sourceType, out var targetType))
                {
                    context.RegisterDocumentEditorFix(
                        $"Add ValueConversionAttribute.",
                        (e, _) => AddAttribute(e, classDeclaration, sourceType, targetType),
                        diagnostic);
                }
            }
        }

        private static void AddAttribute(DocumentEditor editor, ClassDeclarationSyntax classDeclaration, ITypeSymbol inType, ITypeSymbol outType)
        {
            editor.AddAttribute(
                classDeclaration,
                editor.Generator.AddAttributeArguments(
                    Attribute,
                    new[]
                    {
                        editor.Generator.AttributeArgument(
                            editor.Generator.TypeOfExpression(editor.Generator.TypeExpression(inType))),
                        editor.Generator.AttributeArgument(
                            editor.Generator.TypeOfExpression(editor.Generator.TypeExpression(outType))),
                    }));
        }
    }
}