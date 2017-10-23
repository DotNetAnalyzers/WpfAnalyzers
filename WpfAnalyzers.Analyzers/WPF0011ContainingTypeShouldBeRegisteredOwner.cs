namespace WpfAnalyzers
{
    using System.Collections.Immutable;
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
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
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
                context.ContainingSymbol.IsStatic &&
                invocation.TryGetInvokedMethodName(out var name))
            {
                ArgumentSyntax argument;
                if (name.StartsWith("Register") &&
                    invocation.TryGetArgumentAtIndex(2, out argument) &&
                    context.SemanticModel.GetSymbolSafe(invocation, context.CancellationToken) is IMethodSymbol registerMethod)
                {
                    if (registerMethod == KnownSymbol.DependencyProperty.Register ||
                        registerMethod == KnownSymbol.DependencyProperty.RegisterReadOnly ||
                        registerMethod == KnownSymbol.DependencyProperty.RegisterAttached ||
                        registerMethod == KnownSymbol.DependencyProperty.RegisterAttachedReadOnly)
                    {
                        HandleArgument(context, argument);
                    }
                }

                if (name == "AddOwner" &&
                    invocation.TryGetArgumentAtIndex(0, out argument) &&
                    context.SemanticModel.GetSymbolSafe(invocation, context.CancellationToken) is IMethodSymbol addOwner &&
                    addOwner == KnownSymbol.DependencyProperty.AddOwner)
                {
                    HandleArgument(context, argument);
                }

                if (name == "OverrideMetadata" &&
                    invocation.TryGetArgumentAtIndex(0, out argument) &&
                    context.SemanticModel.GetSymbolSafe(invocation, context.CancellationToken) is IMethodSymbol method &&
                    method == KnownSymbol.DependencyProperty.OverrideMetadata)
                {
                    var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
                    if (memberAccess == null)
                    {
                        return;
                    }

                    var containingType = context.ContainingSymbol.ContainingType;
                    var dp = context.SemanticModel.GetSymbolSafe(memberAccess.Expression, context.CancellationToken) as
                        IFieldSymbol;
                    if (!containingType.Is(dp?.ContainingType))
                    {
                        return;
                    }

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

            if (!context.ContainingSymbol.ContainingType.IsSameType(ownerType as INamedTypeSymbol))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, argument.GetLocation(), context.ContainingSymbol.ContainingType));
            }
        }
    }
}