namespace WpfAnalyzers
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

            if (context.Node is InvocationExpressionSyntax invocation &&
                context.SemanticModel.GetSymbolSafe(invocation, context.CancellationToken) is IMethodSymbol method &&
                method.ContainingType != KnownSymbol.DependencyObject)
            {
                ArgumentSyntax argument;
                if (method == KnownSymbol.DependencyProperty.AddOwner ||
                    method == KnownSymbol.DependencyProperty.OverrideMetadata)
                {
                    if (!invocation.TryGetArgumentAtIndex(0, out argument))
                    {
                        return;
                    }
                }
                else if (method == KnownSymbol.DependencyProperty.Register ||
                         method == KnownSymbol.DependencyProperty.RegisterReadOnly)
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
}