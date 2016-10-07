namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RenameMethodCodeFixProvider))]
    [Shared]
    internal class RenameMethodCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(WPF0004ClrMethodForDependencyPropertyShouldMatchRegisteredName.DiagnosticId);

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
                        "Rename property to match registered name.",
                        _ => ApplyFixAsync(context, diagnostic),
                        this.GetType().Name),
                    diagnostic);
            }
        }

        private static async Task<Solution> ApplyFixAsync(CodeFixContext context, Diagnostic diagnostic)
        {
            var document = context.Document;
            var syntaxRoot = await document.GetSyntaxRootAsync(context.CancellationToken)
                                           .ConfigureAwait(false);
            var token = syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start);
            var method = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                        .FirstAncestorOrSelf<MethodDeclarationSyntax>();

            string registeredName;
            string expectedName = null;
            if (method.TryGetDependencyPropertyRegisteredNameFromAttachedGet(out registeredName))
            {
                expectedName = "Get" + registeredName;
            }
            else if (method.TryGetDependencyPropertyRegisteredNameFromAttachedSet(out registeredName))
            {
                expectedName = "Set" + registeredName;
            }

            if (expectedName != null)
            {
                return await RenameHelper.RenameSymbolAsync(document, syntaxRoot, token, expectedName, context.CancellationToken).ConfigureAwait(false);
            }

            return document.Project.Solution;
        }
    }
}