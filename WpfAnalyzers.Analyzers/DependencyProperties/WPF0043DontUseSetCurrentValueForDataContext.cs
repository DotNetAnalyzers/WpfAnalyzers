namespace WpfAnalyzers.DependencyProperties
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

            var invocation = context.Node as InvocationExpressionSyntax;
            if (invocation == null || context.SemanticModel == null)
            {
                return;
            }

            var method = context.SemanticModel.GetSymbolSafe(invocation, context.CancellationToken) as IMethodSymbol;
            if (method != KnownSymbol.DependencyObject.SetCurrentValue)
            {
                return;
            }

            if (!DependencyObject.TryGetSetCurrentValueArguments(invocation, context.SemanticModel, context.CancellationToken, out ArgumentSyntax _, out IFieldSymbol setField, out ArgumentSyntax value))
            {
                return;
            }

            if (setField != KnownSymbol.FrameworkElement.DataContextProperty)
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocation.GetLocation(), setField.Name, value));
        }
    }
}