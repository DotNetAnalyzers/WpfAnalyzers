namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MakeFieldStaticReadonlyCodeFixProvider))]
    [Shared]
    internal class MakeFieldStaticReadonlyCodeFixProvider : DocumentEditorCodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(WPF0030BackingFieldShouldBeStaticReadonly.DiagnosticId);

        /// <inheritdoc/>
        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
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
                        "Make static readonly",
                        (e, _) => ApplyFix(e, fieldDeclaration),
                        this.GetType(),
                    diagnostic);
            }
        }

        private static void ApplyFix(DocumentEditor context, FieldDeclarationSyntax fieldDeclaration)
        {
            context.ReplaceNode(fieldDeclaration, (x, _) =>
            {
                var f = (FieldDeclarationSyntax)x;
                return f.WithModifiers(
                    f.Modifiers
                     .WithStatic()
                     .WithReadOnly());
            });
        }
    }
}
