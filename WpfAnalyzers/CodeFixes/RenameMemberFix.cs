namespace WpfAnalyzers;

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;

using Gu.Roslyn.AnalyzerExtensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Rename;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RenameMemberFix))]
[Shared]
internal class RenameMemberFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
        Descriptors.WPF0001BackingFieldShouldMatchRegisteredName.Id,
        Descriptors.WPF0002BackingFieldShouldMatchRegisteredName.Id,
        Descriptors.WPF0003ClrPropertyShouldMatchRegisteredName.Id,
        Descriptors.WPF0004ClrMethodShouldMatchRegisteredName.Id,
        Descriptors.WPF0005PropertyChangedCallbackShouldMatchRegisteredName.Id,
        Descriptors.WPF0006CoerceValueCallbackShouldMatchRegisteredName.Id,
        Descriptors.WPF0007ValidateValueCallbackCallbackShouldMatchRegisteredName.Id,
        Descriptors.WPF0090RegisterClassHandlerCallbackNameShouldMatchEvent.Id,
        Descriptors.WPF0091AddAndRemoveHandlerCallbackNameShouldMatchEvent.Id,
        Descriptors.WPF0100BackingFieldShouldMatchRegisteredName.Id,
        Descriptors.WPF0102EventDeclarationName.Id);

    public override FixAllProvider? GetFixAllProvider() => null;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var document = context.Document;
        var syntaxRoot = await document.GetSyntaxRootAsync(context.CancellationToken)
                                       .ConfigureAwait(false);
        var semanticModel = await document.GetSemanticModelAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
        foreach (var diagnostic in context.Diagnostics)
        {
            if (syntaxRoot is { } &&
                syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start) is { Parent: { } } token &&
                token.IsKind(SyntaxKind.IdentifierToken) &&
                semanticModel is { } &&
                semanticModel.TryGetSymbol(token, context.CancellationToken, out ISymbol? symbol) &&
                diagnostic.Properties.TryGetValue("ExpectedName", out var newName) &&
                newName is { })
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        $"Rename to: '{newName}'.",
                        cancellationToken => Renamer.RenameSymbolAsync(document.Project.Solution, symbol, newName, document.Project.Solution.Workspace.Options, cancellationToken),
                        this.GetType().FullName),
                    diagnostic);
            }
        }
    }
}
