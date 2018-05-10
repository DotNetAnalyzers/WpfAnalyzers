namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0011ContainingTypeShouldBeRegisteredOwner : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0011";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Containing type should be used as registered owner.",
            messageFormat: "Register containing type: '{0}' as owner.",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "When registering a DependencyProperty register containing type as owner type.",
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
                context.ContainingSymbol.IsStatic)
            {
                if (invocation.TryGetArgumentAtIndex(2, out var argument) &&
                    (DependencyProperty.TryGetRegisterCall(invocation, context.SemanticModel, context.CancellationToken, out _) ||
                     DependencyProperty.TryGetRegisterReadOnlyCall(invocation, context.SemanticModel, context.CancellationToken, out _) ||
                     DependencyProperty.TryGetRegisterAttachedCall(invocation, context.SemanticModel, context.CancellationToken, out _) ||
                     DependencyProperty.TryGetRegisterAttachedReadOnlyCall(invocation, context.SemanticModel, context.CancellationToken, out _)))
                {
                    HandleArgument(context, argument);
                }
                else if (invocation.TryGetArgumentAtIndex(0, out argument))
                {
                    if (DependencyProperty.TryGetAddOwnerCall(invocation, context.SemanticModel, context.CancellationToken, out _))
                    {
                        HandleArgument(context, argument);
                    }
                    else if (DependencyProperty.TryGetOverrideMetadataCall(invocation, context.SemanticModel, context.CancellationToken, out _) &&
                             invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                             BackingFieldOrProperty.TryCreate(context.SemanticModel.GetSymbolSafe(memberAccess.Expression, context.CancellationToken), out var fieldOrProperty) &&
                             context.ContainingSymbol.ContainingType.IsAssignableTo(fieldOrProperty.ContainingType, context.Compilation))
                    {
                        HandleArgument(context, argument);
                    }
                }
            }
        }

        private static void HandleArgument(SyntaxNodeAnalysisContext context, ArgumentSyntax argument)
        {
            if (argument.TryGetTypeofValue(context.SemanticModel, context.CancellationToken, out var ownerType) &&
                !context.ContainingSymbol.ContainingType.IsSameType(ownerType, context.Compilation))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, argument.GetLocation(), context.ContainingSymbol.ContainingType));
            }
        }
    }
}
