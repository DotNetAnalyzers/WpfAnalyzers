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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseContainingTypeAsOwnerCodeFixProvider))]
    [Shared]
    internal class UseContainingTypeAsOwnerCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            WPF0011ContainingTypeShouldBeRegisteredOwner.DiagnosticId,
            WPF0101RegisterContainingTypeAsOwner.DiagnosticId);

        public override FixAllProvider GetFixAllProvider() => DocumentEditorFixAllProvider.Default;

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

                var argument = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                         .FirstAncestorOrSelf<ArgumentSyntax>();
                if (argument == null ||
                    argument.IsMissing ||
                    argument.Expression?.IsKind(SyntaxKind.TypeOfExpression) != true)
                {
                    continue;
                }

                var type = semanticModel.GetDeclaredSymbolSafe(argument.FirstAncestorOrSelf<TypeDeclarationSyntax>(), context.CancellationToken);
                if (type == null)
                {
                    continue;
                }

                var containingTypeName = type.ToMinimalDisplayString(semanticModel, argument.SpanStart, SymbolDisplayFormat.MinimallyQualifiedFormat);
                context.RegisterDocumentEditorFix(
                        $"Use containing type: {containingTypeName}.",
                        (e, _) => Fix(e, (TypeOfExpressionSyntax)argument.Expression, containingTypeName),
                    diagnostic);
            }
        }

        private static void Fix(DocumentEditor editor, TypeOfExpressionSyntax typeOfExpression, string containingTypeName)
        {
            editor.ReplaceNode(
                typeOfExpression,
                (x, g) => ((TypeOfExpressionSyntax)x).WithType(SyntaxFactory.ParseTypeName(containingTypeName)));
        }
    }
}