namespace WpfAnalyzers
{
    using System;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(StandardDocsFix))]
    [Shared]
    [Obsolete("Use documentation fix")]
    internal class StandardDocsFix : DocumentEditorCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.WPF0108DocumentRoutedEventBackingMember.Id);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var document = context.Document;
            var syntaxRoot = await document.GetSyntaxRootAsync(context.CancellationToken)
                                           .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNodeOrAncestor(diagnostic, out MemberDeclarationSyntax? member) &&
                    diagnostic.Properties.TryGetValue(nameof(CrefParameterSyntax), out var name) &&
                    TryGetDocumentation(diagnostic, name, out var summary))
                {
                    context.RegisterCodeFix(
                        "Add standard documentation.",
                        (editor, _) => editor.ReplaceNode(member, x => x.WithDocumentationText(summary)),
                        this.GetType(),
                        diagnostic);
                }
            }
        }

        private static bool TryGetDocumentation(Diagnostic diagnostic, string name, [NotNullWhen(true)] out string? summaryText)
        {
            if (diagnostic.Id == Descriptors.WPF0060DocumentDependencyPropertyBackingMember.Id)
            {
                summaryText = $"/// <summary>Identifies the <see cref=\"{name}\"/> dependency property.</summary>";
                return true;
            }

            if (diagnostic.Id == Descriptors.WPF0108DocumentRoutedEventBackingMember.Id)
            {
                summaryText = $"/// <summary>Identifies the <see cref=\"{name}\"/> routed event.</summary>";
                return true;
            }

            summaryText = null;
            return false;
        }
    }
}
