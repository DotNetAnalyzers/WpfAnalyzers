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
        private const string Title = "Containing type should be used as registered owner.";
        private const string MessageFormat = "Register containing type: '{0}' as owner.";
        private const string Description = "When registering a DependencyProperty register containing type as owner type.";
        private static readonly string HelpLink = WpfAnalyzers.HelpLink.ForId(DiagnosticId);

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
                                                                      DiagnosticId,
                                                                      Title,
                                                                      MessageFormat,
                                                                      AnalyzerCategory.DependencyProperties,
                                                                      DiagnosticSeverity.Warning,
                                                                      AnalyzerConstants.EnabledByDefault,
                                                                      Description,
                                                                      HelpLink);

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

            ITypeSymbol ownerType;
            if (!argument.TryGetTypeofValue(context.SemanticModel, context.CancellationToken, out ownerType))
            {
                return;
            }

            if (!context.ContainingSymbol.ContainingType.IsSameType(ownerType))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, argument.GetLocation(), context.ContainingSymbol.ContainingType));
            }
        }
    }
}