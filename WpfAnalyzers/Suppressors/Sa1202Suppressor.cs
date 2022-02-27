namespace WpfAnalyzers.Suppressors;

using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class Sa1202Suppressor : DiagnosticSuppressor
{
    private static readonly SuppressionDescriptor Descriptor = new(nameof(Sa1202Suppressor), "SA1202", "Does not handle attached properties correctly.");

    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; } = ImmutableArray.Create(
        Descriptor);

    public override void ReportSuppressions(SuppressionAnalysisContext context)
    {
        foreach (var diagnostic in context.ReportedDiagnostics)
        {
            if (diagnostic.Location is { SourceTree: { } tree } &&
                tree.GetRoot(context.CancellationToken) is { } root)
            {
                switch (root.FindNode(diagnostic.Location.SourceSpan))
                {
                    case VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Type: { } type } }
                        when type == KnownSymbols.DependencyProperty:
                        context.ReportSuppression(Suppression.Create(Descriptor, diagnostic));
                        break;
                    case MethodDeclarationSyntax method
                        when context.GetSemanticModel(tree) is { } semanticModel &&
                             GetAttached.Match(method, semanticModel, context.CancellationToken) is { }:
                        context.ReportSuppression(Suppression.Create(Descriptor, diagnostic));
                        break;
                }
            }
        }
    }
}