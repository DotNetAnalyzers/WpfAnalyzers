namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0015RegisteredOwnerTypeMustBeDependencyObject : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0015";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Registered owner type must inherit DependencyObject.",
            messageFormat: "Maybe you intended to use '{0}'?",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "When registering a DependencyProperty owner type must be a subclass of DependencyObject.",
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
            if (invocation == null ||
                invocation.IsMissing)
            {
                return;
            }

            var methodSymbol = context.SemanticModel.GetSymbolSafe(invocation, context.CancellationToken) as IMethodSymbol;
            if (methodSymbol == null ||
                methodSymbol.ContainingType == KnownSymbol.DependencyObject)
            {
                return;
            }

            ArgumentSyntax argument;
            if (methodSymbol == KnownSymbol.DependencyProperty.AddOwner ||
                methodSymbol == KnownSymbol.DependencyProperty.OverrideMetadata)
            {
                if (!invocation.TryGetArgumentAtIndex(0, out argument))
                {
                    return;
                }
            }
            else if (methodSymbol == KnownSymbol.DependencyProperty.Register ||
                     methodSymbol == KnownSymbol.DependencyProperty.RegisterReadOnly)
            {
                if (!invocation.TryGetArgumentAtIndex(2, out argument))
                {
                    return;
                }
            }
            else
            {
                return;
            }

            if (!argument.TryGetTypeofValue(context.SemanticModel, context.CancellationToken, out ITypeSymbol ownerType))
            {
                return;
            }

            if (!ownerType.Is(KnownSymbol.DependencyObject))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, argument.GetLocation(), KnownSymbol.DependencyProperty.RegisterAttached.Name));
            }
        }
    }
}