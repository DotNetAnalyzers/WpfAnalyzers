namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0043DontUseSetCurrentValueForDataContext : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0043";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Don't set DataContext using SetCurrentValue.",
            messageFormat: "Use SetValue({0}, {1})",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "Set DataContext using SetValue.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(HandleInvocation, SyntaxKind.InvocationExpression);
        }

        private static void HandleInvocation(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.Node is InvocationExpressionSyntax invocation)
            {
                if (DependencyObject.TryGetSetCurrentValueCall(invocation, context.SemanticModel, context.CancellationToken, out _) &&
                    context.SemanticModel.GetSymbolSafe(invocation.ArgumentList.Arguments[0].Expression, context.CancellationToken) is IFieldSymbol setField &&
                    setField == KnownSymbol.FrameworkElement.DataContextProperty)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocation.GetLocation(), setField.Name, invocation.ArgumentList.Arguments[1]));
                }
            }
        }
    }
}