namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
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

            foreach (var diagnostic in context.Diagnostics)
            {
                var token = syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start);
                if (string.IsNullOrEmpty(token.ValueText))
                {
                    continue;
                }

                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Register containing type as owner.",
                        _ => ApplyFixAsync(context, diagnostic),
                        this.GetType().Name),
                    diagnostic);
            }
        }

        private static async Task<Document> ApplyFixAsync(CodeFixContext context, Diagnostic diagnostic)
        {
            var document = context.Document;
            var syntaxRoot = await document.GetSyntaxRootAsync(context.CancellationToken)
                                           .ConfigureAwait(false);

            var argument = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                     .FirstAncestorOrSelf<ArgumentSyntax>();

            if (argument == null)
            {
                return document;
            }

            var typeOfExpression = argument.Expression as TypeOfExpressionSyntax;
            var containingType = argument.Ancestors()
                             .OfType<TypeDeclarationSyntax>()
                             .FirstOrDefault();

            if (containingType == null || typeOfExpression == null)
            {
                return document;
            }

            var updated = typeOfExpression.WithType(SyntaxFactory.IdentifierName(containingType.Identifier.ValueText));
            return document.WithSyntaxRoot(syntaxRoot.ReplaceNode(typeOfExpression, updated));
        }
    }
}