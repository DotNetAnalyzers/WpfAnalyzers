﻿namespace WpfAnalyzers
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
        private const string Title = "Add standard documentation.";

        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.WPF0060DocumentDependencyPropertyBackingMember.Id,
            Descriptors.WPF0061DocumentClrMethod.Id,
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
                                Title,
                                editor => editor.ReplaceNode(element, x => Parse.XmlElementSyntax(text)),
                                Title,
                                diagnostic);
                            break;
                        case XmlEmptyElementSyntax element:
                            context.RegisterCodeFix(
                                Title,
                                editor => editor.ReplaceNode(element, x => Parse.XmlEmptyElementSyntax(text)),
                                Title,
                                diagnostic);
                            break;
                        case XmlNameSyntax name:
                            context.RegisterCodeFix(
                                Title,
                                editor => editor.ReplaceNode(name, x => SyntaxFactory.XmlName(text)),
                                Title,
                                diagnostic);
                            break;
                        case DocumentationCommentTriviaSyntax element
                            when text.StartsWith("<summary>"):
                            context.RegisterCodeFix(
                                Title,
                                editor => editor.ReplaceNode(element, x => x.WithSummary(Parse.XmlElementSyntax(text))),
                                Title,
                                diagnostic);
                            break;
                        case DocumentationCommentTriviaSyntax element:
                            context.RegisterCodeFix(
                                Title,
                                editor => editor.ReplaceNode(element, x => Parse.DocumentationCommentTriviaSyntax(text)),
                                Title,
                                diagnostic);
                            break;
                        case IdentifierNameSyntax identifierName:
                            context.RegisterCodeFix(
                                Title,
                                editor => editor.ReplaceNode(identifierName, x => x.WithIdentifier(SyntaxFactory.Identifier(text))),
                                Title,
                                diagnostic);
                            break;
                        case TypeSyntax typeSyntax
                            when typeSyntax.Parent is MethodDeclarationSyntax method &&
                                 method.TryGetDocumentationComment(out var comment):
                            context.RegisterCodeFix(
                                Title,
                                editor => editor.ReplaceNode(comment, x => x.WithReturns(Parse.XmlElementSyntax(text))),
                                Title,
                                diagnostic);
                            break;
                        case MethodDeclarationSyntax method:
                            context.RegisterCodeFix(
                                Title,
                                editor => editor.ReplaceNode(method, x => x.WithDocumentationText(text)),
                                Title,
                                diagnostic);
                            break;
                        case PropertyDeclarationSyntax property:
                            context.RegisterCodeFix(
                                Title,
                                editor => editor.ReplaceNode(property, x => x.WithDocumentationText(text)),
                                Title,
                                diagnostic);
                            break;
                        case VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Parent: FieldDeclarationSyntax field } }:
                            context.RegisterCodeFix(
                                Title,
                                editor => editor.ReplaceNode(field, x => x.WithDocumentationText(text)),
                                Title,
                                diagnostic);
                            break;
                        case ParameterSyntax { Parent: ParameterListSyntax { Parent: MethodDeclarationSyntax method } } parameter
                            when method.TryGetDocumentationComment(out var comment):
                            context.RegisterCodeFix(
                                Title,
                                editor => editor.ReplaceNode(comment, x => x.WithParamText(parameter.Identifier.ValueText, text)),
                                Title,
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