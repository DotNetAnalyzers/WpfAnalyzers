namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
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
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
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

            if (context.Node is InvocationExpressionSyntax invocation)
            {
                if ((DependencyProperty.TryGetOverrideMetadataCall(invocation, context.SemanticModel, context.CancellationToken, out _) ||
                     DependencyProperty.TryGetAddOwnerCall(invocation, context.SemanticModel, context.CancellationToken, out _)) &&
                    invocation.TryGetArgumentAtIndex(0, out var argument))
                {
                    HandleArgument(context, argument);
                }

                if ((DependencyProperty.TryGetRegisterCall(invocation, context.SemanticModel, context.CancellationToken, out _) ||
                     DependencyProperty.TryGetRegisterReadOnlyCall(invocation, context.SemanticModel, context.CancellationToken, out _)) &&
                    invocation.TryGetArgumentAtIndex(2, out argument))
                {
                    HandleArgument(context, argument);
                }
            }
        }

        private static void HandleArgument(SyntaxNodeAnalysisContext context, ArgumentSyntax argument)
        {
            if (!argument.TryGetTypeofValue(context.SemanticModel, context.CancellationToken, out var ownerType))
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
