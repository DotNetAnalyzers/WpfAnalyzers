namespace WpfAnalyzers
{
    using System;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DocumentationFix))]
    [Shared]
    internal class DocumentationFix : DocumentEditorCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.WPF0060DocumentDependencyPropertyBackingMember.Id,
            Descriptors.WPF0062DocumentPropertyChangedCallback.Id);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                                   .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (diagnostic.Properties.TryGetValue(nameof(DocComment), out var text))
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
                        case XmlEmptyElementSyntax element:
                            context.RegisterCodeFix(
                                "Add standard documentation.",
                                (editor, _) => editor.ReplaceNode(element, x => Parse.XmlEmptyElementSyntax(text)),
                                this.GetType(),
                                diagnostic);
                            break;
                        case XmlNameSyntax name:
                            context.RegisterCodeFix(
                                "Add standard documentation.",
                                (editor, _) => editor.ReplaceNode(name, x => SyntaxFactory.XmlName(text)),
                                this.GetType(),
                                diagnostic);
                            break;
                        case DocumentationCommentTriviaSyntax element:
                            context.RegisterCodeFix(
                                "Add standard documentation.",
                                (editor, __) =>
                                {
                                    if (text.StartsWith("<summary>"))
                                    {
                                        _ = editor.ReplaceNode(element, x => x.WithSummary(Parse.XmlElementSyntax(text)));
                                    }
                                    else
                                    {
                                        _ = editor.ReplaceNode(element, x => Parse.DocumentationCommentTriviaSyntax(text));
                                    }
                                },
                                this.GetType(),
                                diagnostic);
                            break;
                        case IdentifierNameSyntax identifierName:
                            context.RegisterCodeFix(
                                "Add standard documentation.",
                                (editor, _) => editor.ReplaceNode(identifierName, x => x.WithIdentifier(SyntaxFactory.Identifier(text))),
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
                        case PropertyDeclarationSyntax property:
                            context.RegisterCodeFix(
                                "Add standard documentation.",
                                (editor, _) => editor.ReplaceNode(property, x => x.WithDocumentationText(text)),
                                this.GetType(),
                                diagnostic);
                            break;
                        case VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Parent: FieldDeclarationSyntax field } }:
                            context.RegisterCodeFix(
                                "Add standard documentation.",
                                (editor, _) => editor.ReplaceNode(field, x => x.WithDocumentationText(text)),
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
                        case { } node:
                            throw new NotSupportedException($"Not handling node type: {node.Kind()}");
                    }
                }
            }
        }
    }
}
