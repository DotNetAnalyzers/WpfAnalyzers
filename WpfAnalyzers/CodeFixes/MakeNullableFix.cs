namespace WpfAnalyzers;

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Gu.Roslyn.CodeFixExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CastFix))]
[Shared]
internal class MakeNullableFix : DocumentEditorCodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
        Descriptors.WPF0024ParameterShouldBeNullable.Id);

    protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
    {
        var document = context.Document;
        var syntaxRoot = await document.GetSyntaxRootAsync(context.CancellationToken)
                                       .ConfigureAwait(false);
        foreach (var diagnostic in context.Diagnostics)
        {
            if (syntaxRoot is { } &&
                syntaxRoot.TryFindNode(diagnostic, out TypeSyntax? typeSyntax))
            {
                context.RegisterCodeFix(
                    $"Change type to: {typeSyntax}?.",
                    (e, _) => e.ReplaceNode(
                        typeSyntax,
                        x => SyntaxFactory.NullableType(x).WithTriviaFrom(x)),
                    this.GetType().FullName,
                    diagnostic);
            }
        }
    }
}
