namespace WpfAnalyzers.Suppressors
{
    using System.Collections.Immutable;

    using Gu.Roslyn.AnalyzerExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class Sa1202Suppressor : DiagnosticSuppressor
    {
        private static readonly SuppressionDescriptor Descriptor = new SuppressionDescriptor(nameof(Sa1202Suppressor), "SA1202", "Always wrong here.");

        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; } = ImmutableArray.Create(
            Descriptor);

        public override void ReportSuppressions(SuppressionAnalysisContext context)
        {
            foreach (var diagnostic in context.ReportedDiagnostics)
            {
                if (diagnostic.Location is { SourceTree: { } tree } &&
                    tree.GetRoot(context.CancellationToken) is { } root &&
                    root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true) is { } node &&
                    node.TryFirstAncestorOrSelf(out IdentifierNameSyntax? identifierName) &&
                    context.GetSemanticModel(identifierName.SyntaxTree) is { } semanticModel)
                {
                    if (semanticModel.TryGetSymbol(identifierName, context.CancellationToken, out var symbol) &&
                        FieldOrProperty.TryCreate(symbol, out var fieldOrProperty) &&
                       (fieldOrProperty.Type == KnownSymbols.DependencyProperty ||
                        fieldOrProperty.Type == KnownSymbols.DependencyPropertyKey))
                    {
                        context.ReportSuppression(Suppression.Create(Descriptor, diagnostic));
                    }

                    if (identifierName.Parent is MethodDeclarationSyntax method &&
                        (SetAttached.Match(method, semanticModel, context.CancellationToken) is { } ||
                         GetAttached.Match(method, semanticModel, context.CancellationToken) is { }))
                    {
                        context.ReportSuppression(Suppression.Create(Descriptor, diagnostic));
                    }
                }
            }
        }
    }
}
