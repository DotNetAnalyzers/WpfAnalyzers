namespace WpfAnalyzers;

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;

using Gu.Roslyn.CodeFixExtensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AttachedPropertyBrowsableForTypeArgumentFix))]
[Shared]
internal class AttachedPropertyBrowsableForTypeArgumentFix : DocumentEditorCodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
        Descriptors.WPF0034AttachedPropertyBrowsableForTypeAttributeArgument.Id);

    protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
    {
        var document = context.Document;
        var syntaxRoot = await document.GetSyntaxRootAsync(context.CancellationToken)
                                       .ConfigureAwait(false);
        foreach (var diagnostic in context.Diagnostics)
        {
            if (syntaxRoot is { } &&
                syntaxRoot.TryFindNode(diagnostic, out AttributeArgumentSyntax? argument) &&
                argument is { Expression: TypeOfExpressionSyntax { Type: { } type }, Parent: AttributeArgumentListSyntax { Parent: AttributeSyntax { Parent: AttributeListSyntax { Parent: MethodDeclarationSyntax { ParameterList.Parameters: { Count: 1 } parameters } } } } } &&
                parameters[0] is { Type: { } toType })
            {
                context.RegisterCodeFix(
                    $"Change type to {toType}.",
                    (editor, _) => editor.ReplaceNode(
                        type,
                        x => toType.WithTriviaFrom(x)),
                    this.GetType(),
                    diagnostic);
            }
        }
    }
}
