namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MakeFieldStaticReadonlyFix))]
    [Shared]
    internal class MakeFieldStaticReadonlyFix : DocumentEditorCodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            WPF0030BackingFieldShouldBeStaticReadonly.DiagnosticId,
            WPF0107BackingMemberShouldBeStaticReadonly.DiagnosticId,
            WPF0123BackingMemberShouldBeStaticReadonly.DiagnosticId);

        /// <inheritdoc/>
        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNodeOrAncestor(diagnostic, out FieldDeclarationSyntax fieldDeclaration))
                {
                    context.RegisterCodeFix(
                        "Make static readonly",
                        (e, _) => e.ReplaceNode(
                            fieldDeclaration,
                            x => x.WithModifiers(x.Modifiers.WithStatic().WithReadOnly())),
                        this.GetType(),
                        diagnostic);
                }
            }
        }
    }
}
