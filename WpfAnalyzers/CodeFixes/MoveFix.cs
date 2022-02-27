namespace WpfAnalyzers;

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;

using Gu.Roslyn.AnalyzerExtensions;
using Gu.Roslyn.CodeFixExtensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MoveFix))]
[Shared]
internal class MoveFix : DocumentEditorCodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
        Descriptors.WPF0031FieldOrder.Id);

    protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
    {
        var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                      .ConfigureAwait(false);
        foreach (var diagnostic in context.Diagnostics)
        {
            if (syntaxRoot is { } &&
                syntaxRoot.TryFindNodeOrAncestor(diagnostic, out MemberDeclarationSyntax? toMove) &&
                diagnostic.AdditionalLocations.TrySingle(out var additionalLocation) &&
                syntaxRoot.TryFindNodeOrAncestor(additionalLocation, out MemberDeclarationSyntax? member))
            {
                context.RegisterCodeFix(
                    "Move",
                    (e, _) => e.MoveBefore(toMove, member),
                    nameof(MoveFix),
                    diagnostic);
            }
        }
    }
}
