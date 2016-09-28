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
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(WA1202FieldMustBeStaticReadOnly.DiagnosticId);

        /// <inheritdoc/>
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Make static readonly",
                        _ => Task.FromResult(FixDocument(context, diagnostic, syntaxRoot)),
                        nameof(MakeFieldStaticReadonlyCodeFixProvider)),
                    diagnostic);
            }
        }

        private static Document FixDocument(CodeFixContext context, Diagnostic diagnostic, SyntaxNode syntaxRoot)
        {
            SyntaxNode updated;
            if (TryFix(diagnostic, syntaxRoot, out updated))
            {
                return context.Document.WithSyntaxRoot(updated);
            }

            return context.Document;
        }

        private static bool TryFix(Diagnostic diagnostic, SyntaxNode syntaxRoot, out SyntaxNode result)
        {
            result = syntaxRoot;
            var fieldDeclaration = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                             .FirstAncestorOrSelf<FieldDeclarationSyntax>();
            if (fieldDeclaration == null || fieldDeclaration.IsMissing)
            {
                return false;
            }

            var updatedModifiers = fieldDeclaration.Modifiers
                                                   .WithStatic()
                                                   .WithReadOnly();
            var updatedDeclaration = fieldDeclaration.WithModifiers(updatedModifiers);
            result = syntaxRoot.ReplaceNode(fieldDeclaration, updatedDeclaration);
            return true;
        }
    }
}
