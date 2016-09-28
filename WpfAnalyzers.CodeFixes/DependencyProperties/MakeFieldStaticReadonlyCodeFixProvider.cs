namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;
    using System.Composition;
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
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(WA1202FieldMustBeStaticReadOnly.DiagnosticId);

        /// <inheritdoc/>
        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Make static readonly",
                        _ => ApplyFixAsync(context, diagnostic),
                        nameof(MakeFieldStaticReadonlyCodeFixProvider)),
                    diagnostic);
            }

            return FinishedTasks.Task;
        }

        private static async Task<Document> ApplyFixAsync(CodeFixContext context, Diagnostic diagnostic)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                              .ConfigureAwait(false);
            var fieldDeclaration = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                             .FirstAncestorOrSelf<FieldDeclarationSyntax>();
            if (fieldDeclaration == null || fieldDeclaration.IsMissing)
            {
                return context.Document;
            }

            var updatedModifiers = fieldDeclaration.Modifiers
                                                   .WithStatic()
                                                   .WithReadOnly();
            var updatedDeclaration = fieldDeclaration.WithModifiers(updatedModifiers);
            return context.Document.WithSyntaxRoot(syntaxRoot.ReplaceNode(fieldDeclaration, updatedDeclaration));
        }
    }
}
