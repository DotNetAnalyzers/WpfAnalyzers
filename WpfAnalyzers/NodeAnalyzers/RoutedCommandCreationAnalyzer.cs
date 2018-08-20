namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class RoutedCommandCreationAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            WPF0121RegisterContainingTypeAsOwnerForRoutedCommand.Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(x => Handle(x), SyntaxKind.ObjectCreationExpression);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is ObjectCreationExpressionSyntax objectCreation &&
                (objectCreation.Type == KnownSymbol.RoutedCommand || objectCreation.Type == KnownSymbol.RoutedUICommand) &&
                context.SemanticModel.TryGetSymbol(objectCreation, context.CancellationToken, out var ctor))
            {
                if (ctor.TryFindParameter("ownerType", out var parameter))
                {
                    if (objectCreation.TryFindArgument(parameter, out var ownerTypeArg) &&
                        ownerTypeArg.TryGetTypeofValue(context.SemanticModel, context.CancellationToken, out var type) &&
                        !type.Equals(context.ContainingSymbol.ContainingType))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                                                     WPF0121RegisterContainingTypeAsOwnerForRoutedCommand.Descriptor,
                                                     ownerTypeArg.GetLocation(),
                                                     context.ContainingSymbol.ContainingType.ToMinimalDisplayString(context.SemanticModel, objectCreation.SpanStart)));
                    }
                }
                else
                {
                    // context.ReportDiagnostic(Diagnostic.Create("Register owner")));
                }
            }
        }
    }
}