namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DocumentClrMethodFix))]
    [Shared]
    internal class DocumentationFix : DocumentEditorCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.WPF0062DocumentPropertyChangedCallback.Id);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var document = context.Document;
            var syntaxRoot = await document.GetSyntaxRootAsync(context.CancellationToken)
                                           .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (diagnostic.Properties.TryGetValue(nameof(Descriptors.WPF0062DocumentPropertyChangedCallback), out var text))
                {
                    switch (syntaxRoot.FindNode(diagnostic.Location.SourceSpan, findInsideTrivia: true, getInnermostNodeForTie: true))
                    {
                        case XmlElementSyntax element:
                            context.RegisterCodeFix(
                                "Add standard documentation.",
                                (editor, _) => editor.ReplaceNode(element, x => Parse.XmlElementSyntax(text)),
                                this.GetType(),
                                diagnostic);
                            break;
                        case DocumentationCommentTriviaSyntax element:
                            context.RegisterCodeFix(
                                "Add standard documentation.",
                                (editor, _) => editor.ReplaceNode(element, x => Parse.DocumentationCommentTriviaSyntax(text)),
                                this.GetType(),
                                diagnostic);
                            break;
                        case MethodDeclarationSyntax method:
                            context.RegisterCodeFix(
                                "Add standard documentation.",
                                (editor, _) => editor.ReplaceNode(method, x => x.WithDocumentationText(text)),
                                this.GetType(),
                                diagnostic);
                            break;
                        case ParameterSyntax { Parent: ParameterListSyntax { Parent: MethodDeclarationSyntax method } } parameter
                            when method.TryGetDocumentationComment(out var comment):
                            context.RegisterCodeFix(
                                "Add standard documentation.",
                                (editor, _) => editor.ReplaceNode(comment, x => x.WithParamText(parameter.Identifier.ValueText, text)),
                                this.GetType(),
                                diagnostic);
                            break;

                    }
                }
            }
        }
    }
}
