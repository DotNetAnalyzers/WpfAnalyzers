namespace WpfAnalyzers.DependencyProperties
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

            var invocation = context.Node as InvocationExpressionSyntax;
            if (invocation == null ||
                invocation.IsMissing)
            {
                return;
            }

            var methodSymbol = context.SemanticModel.GetSymbolSafe(invocation, context.CancellationToken) as IMethodSymbol;
            if (methodSymbol == null ||
                methodSymbol.ContainingType != KnownSymbol.DependencyProperty)
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

                if (methodSymbol == KnownSymbol.DependencyProperty.OverrideMetadata)
                {
                    var containingType = context.ContainingSymbol.ContainingType;
                    var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
                    if (memberAccess == null)
                    {
                        return;
                    }

                    var dp = context.SemanticModel.GetSymbolSafe(memberAccess.Expression, context.CancellationToken) as IFieldSymbol;
                    if (!containingType.Is(dp?.ContainingType))
                    {
                        return;
                    }
                }
            }
            else if (methodSymbol == KnownSymbol.DependencyProperty.Register ||
                     methodSymbol == KnownSymbol.DependencyProperty.RegisterReadOnly ||
                     methodSymbol == KnownSymbol.DependencyProperty.RegisterAttached ||
                     methodSymbol == KnownSymbol.DependencyProperty.RegisterAttachedReadOnly)
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

            if (!context.ContainingSymbol.ContainingType.IsSameType(ownerType as INamedTypeSymbol))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, argument.GetLocation(), context.ContainingSymbol.ContainingType));
            }
        }
    }
}