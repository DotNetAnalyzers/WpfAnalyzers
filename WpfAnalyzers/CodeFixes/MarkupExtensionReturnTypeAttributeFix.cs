﻿namespace WpfAnalyzers;

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Gu.Roslyn.CodeFixExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MarkupExtensionReturnTypeAttributeFix))]
[Shared]
internal class MarkupExtensionReturnTypeAttributeFix : DocumentEditorCodeFixProvider
{
    private static readonly AttributeSyntax Attribute = SyntaxFactory
                                                        .Attribute(SyntaxFactory.ParseName("System.Windows.Markup.MarkupExtensionReturnTypeAttribute")).WithSimplifiedNames();

    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
        Descriptors.WPF0080MarkupExtensionDoesNotHaveAttribute.Id);

    protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
    {
        var document = context.Document;
        var syntaxRoot = await document.GetSyntaxRootAsync(context.CancellationToken)
                                       .ConfigureAwait(false);
        var semanticModel = await document.GetSemanticModelAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
        foreach (var diagnostic in context.Diagnostics)
        {
            if (syntaxRoot is { } &&
                syntaxRoot.TryFindNodeOrAncestor(diagnostic, out ClassDeclarationSyntax? classDeclaration) &&
                semanticModel is { } &&
                MarkupExtension.TryGetReturnType(classDeclaration, semanticModel, context.CancellationToken, out var returnType))
            {
                context.RegisterCodeFix(
                    "Add MarkupExtensionReturnTypeAttribute.",
                    (e, _) => AddAttribute(e),
                    "Add MarkupExtensionReturnTypeAttribute.",
                    diagnostic);

                void AddAttribute(DocumentEditor editor)
                {
                    editor.AddAttribute(
                        classDeclaration,
                        editor.Generator.AddAttributeArguments(
                            Attribute,
                            new[] { editor.Generator.AttributeArgument(editor.Generator.TypeOfExpression(editor.Generator.TypeExpression(returnType))) }));
                }
            }
        }
    }
}
