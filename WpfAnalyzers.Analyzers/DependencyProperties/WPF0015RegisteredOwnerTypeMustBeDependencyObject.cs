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
        private const string Title = "Registered owner type must inherit DependencyObject.";
        private const string MessageFormat = "Maybe you intended to use '{0}'?";
        private const string Description = "When registering a DependencyProperty owner type must be a subclass of DependencyObject.";
        private static readonly string HelpLink = WpfAnalyzers.HelpLink.ForId(DiagnosticId);

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
                                                                      DiagnosticId,
                                                                      Title,
                                                                      MessageFormat,
                                                                      AnalyzerCategory.DependencyProperties,
                                                                      DiagnosticSeverity.Error,
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
            context.RegisterSyntaxNodeAction(HandleDeclaration, SyntaxKind.InvocationExpression);
        }

        private static void HandleDeclaration(SyntaxNodeAnalysisContext context)
        {
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
            ITypeSymbol ownerType;
            if (methodSymbol == KnownSymbol.DependencyProperty.AddOwner)
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

            if (!argument.TryGetTypeofValue(context.SemanticModel, context.CancellationToken, out ownerType))
            {
                return;
            }

            var field = context.ContainingSymbol as IFieldSymbol;
            if (field == null ||
                !(DependencyProperty.IsPotentialDependencyPropertyBackingField(field) ||
                  DependencyProperty.IsPotentialDependencyPropertyKeyBackingField(field)))
            {
                return;
            }

            if (!field.ContainingType.Is(KnownSymbol.DependencyObject))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, argument.GetLocation(), KnownSymbol.DependencyProperty.RegisterAttached.Name));
            }
        }
    }
}