namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Threading.Tasks;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddTemplatePartAttributeFix))]
    [Shared]
    internal class AddTemplatePartAttributeFix : DocumentEditorCodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            WPF0130UseTemplatePartAttribute.DiagnosticId);

        /// <inheritdoc/>
        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var document = context.Document;
            var syntaxRoot = await document.GetSyntaxRootAsync(context.CancellationToken)
                                           .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNodeOrAncestor<ClassDeclarationSyntax>(diagnostic, out var classDeclaration))
                {
                    if (diagnostic.Properties.TryGetValue(nameof(AttributeListSyntax), out var attribute))
                    {
                        context.RegisterCodeFix(
                            $"Add {attribute}.",
                            (e, _) =>
                            {
                                e.ReplaceNode(
                                    classDeclaration,
                                    classDeclaration.WithAttributeLists(
                                        classDeclaration.AttributeLists.Add(
                                            ParseAttributeList(attribute))));
                            },
                            $"[TemplatePart].",
                            diagnostic);
                    }
                }
            }
        }

        private static AttributeListSyntax ParseAttributeList(string text)
        {
            var code = $"\r\n    {text}public class Foo {{}}";
            return ((ClassDeclarationSyntax)SyntaxFactory.ParseCompilationUnit(code).Members.Single()).AttributeLists
                                                                                                      .Single()
                                                                                                      .WithSimplifiedNames();
        }
    }
}
