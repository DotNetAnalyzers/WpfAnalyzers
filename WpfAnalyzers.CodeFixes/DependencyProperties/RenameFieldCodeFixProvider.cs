namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RenameFieldCodeFixProvider))]
    [Shared]
    internal class RenameFieldCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(
                WA0001FieldNameMustMatchRegisteredName.DiagnosticId,
                WA1201FieldNameMustMatchRegisteredName.DiagnosticId);

        /// <inheritdoc/>
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var syntaxRoot = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                var token = syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start);
                if (string.IsNullOrEmpty(token.ValueText))
                {
                    continue;
                }

                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Rename field to match registered name.",
                        _ => ApplyFixAsync(context, diagnostic),
                        this.GetType().Name),
                    diagnostic);
            }
        }

        private static async Task<Solution> ApplyFixAsync(CodeFixContext context, Diagnostic diagnostic)
        {
            var document = context.Document;
            var syntaxRoot = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var token = syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start);
            var fieldDeclaration = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                             .FirstAncestorOrSelf<FieldDeclarationSyntax>();
            string registeredName;
            if (fieldDeclaration.TryGetDependencyPropertyRegisteredName(out registeredName))
            {
                var newName = diagnostic.Id == WA0001FieldNameMustMatchRegisteredName.DiagnosticId
                  ? registeredName + "Property"
                  : registeredName + "PropertyKey";

                return await RenameHelper.RenameSymbolAsync(document, syntaxRoot, token, newName, context.CancellationToken)
                                         .ConfigureAwait(false);
            }

            return document.Project.Solution;
        }
    }
}
