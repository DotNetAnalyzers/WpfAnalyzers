namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MakeFieldStaticReadonlyCodeFixProvider))]
    [Shared]
    internal class MakeFieldStaticReadonlyCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(WPF0030BackingFieldShouldBeStaticReadonly.DiagnosticId);

        /// <inheritdoc/>
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                var token = syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start);
                if (string.IsNullOrEmpty(token.ValueText))
                {
                    continue;
                }

                var fieldDeclaration = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                     .FirstAncestorOrSelf<FieldDeclarationSyntax>();
                if (fieldDeclaration == null || fieldDeclaration.IsMissing)
                {
                    continue;
                }

                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Make static readonly",
                        cancellationToken => ApplyFixAsync(cancellationToken, context, fieldDeclaration),
                        this.GetType().FullName),
                    diagnostic);
            }
        }

        private static async Task<Document> ApplyFixAsync(CancellationToken cancellationToken, CodeFixContext context, FieldDeclarationSyntax fieldDeclaration)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(cancellationToken)
                              .ConfigureAwait(false);

            var updatedModifiers = fieldDeclaration.Modifiers
                                                   .WithStatic()
                                                   .WithReadOnly();
            var updatedDeclaration = fieldDeclaration.WithModifiers(updatedModifiers);
            return context.Document.WithSyntaxRoot(syntaxRoot.ReplaceNode(fieldDeclaration, updatedDeclaration));
        }
    }
}
