namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseContainingTypeAsOwnerCodeFixProvider))]
    [Shared]
    internal class UseContainingTypeAsOwnerCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(WPF0011ContainingTypeShouldBeRegisteredOwner.DiagnosticId);

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
                var typeSyntax = SyntaxFactory.ParseTypeName(containingTypeName);
                context.RegisterCodeFix(
                    CodeAction.Create(
                        $"Use containing type: {containingTypeName}.",
                        _ => ApplyFixAsync(context, (TypeOfExpressionSyntax)argument.Expression, typeSyntax),
                        this.GetType().FullName),
                    diagnostic);
            }
        }

        private static async Task<Document> ApplyFixAsync(CodeFixContext context, TypeOfExpressionSyntax typeOfExpression, TypeSyntax containingType)
        {
            var document = context.Document;
            var syntaxRoot = await document.GetSyntaxRootAsync(context.CancellationToken)
                                           .ConfigureAwait(false);

            var updated = typeOfExpression.WithType(containingType);
            return document.WithSyntaxRoot(syntaxRoot.ReplaceNode(typeOfExpression, updated));
        }
    }
}