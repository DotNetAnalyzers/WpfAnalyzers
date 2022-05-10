namespace WpfAnalyzers;

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;

using Gu.Roslyn.CodeFixExtensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseCorrectDelegateFix))]
[Shared]
internal class UseCorrectDelegateFix : DocumentEditorCodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
        Descriptors.WPF0092WrongDelegateType.Id);

    protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
    {
        var document = context.Document;
        var syntaxRoot = await document.GetSyntaxRootAsync(context.CancellationToken)
                                       .ConfigureAwait(false);

        foreach (var diagnostic in context.Diagnostics)
        {
            if (syntaxRoot is { } &&
                syntaxRoot.TryFindNodeOrAncestor(diagnostic, out ArgumentSyntax? argument) &&
                diagnostic.Properties.TryGetValue(nameof(ITypeSymbol), out var typeName) &&
                typeName is { })
            {
                switch (argument.Expression)
                {
                    case ObjectCreationExpressionSyntax { Type: { } createdType }:
                        context.RegisterCodeFix(
                            $"Use: {typeName}.",
                            (editor, _) => editor.ReplaceNode(
                                createdType,
                                x => SyntaxFactory.ParseTypeName(typeName).WithTriviaFrom(x)),
                            "Use correct delegate type",
                            diagnostic);
                        break;
                }
            }
        }
    }
}
