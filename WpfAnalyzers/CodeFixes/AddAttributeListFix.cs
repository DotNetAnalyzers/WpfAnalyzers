namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddAttributeListFix))]
    [Shared]
    internal class AddAttributeListFix : DocumentEditorCodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.WPF0130UseTemplatePartAttribute.Id);

        /// <inheritdoc/>
        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var document = context.Document;
            var syntaxRoot = await document.GetSyntaxRootAsync(context.CancellationToken)
                                           .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNodeOrAncestor<ClassDeclarationSyntax>(diagnostic, out var classDeclaration) &&
                    diagnostic.Properties.TryGetValue(nameof(AttributeListSyntax), out var attribute))
                {
                    context.RegisterCodeFix(
                        $"Add {attribute}.",
                        (e, _) => e.ReplaceNode(
                            classDeclaration,
                            x => x.WithLeadingElasticLineFeed().AddAttributeLists(Parse.AttributeList(attribute).WithLeadingTrivia(SyntaxFactory.ElasticSpace).WithSimplifiedNames())),
                        "[TemplatePart]",
                        diagnostic);
                }
            }
        }
    }
}
