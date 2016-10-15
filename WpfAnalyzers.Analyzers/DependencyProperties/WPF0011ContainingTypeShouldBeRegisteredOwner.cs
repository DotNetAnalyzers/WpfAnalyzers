namespace WpfAnalyzers.DependencyProperties
{
    using System;
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
        private const string MessageFormat = "DependencyProperty '{0}' must be registered for containing type: '{1}'";
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

            var methodSymbol = context.SemanticModel.GetSymbolInfo(invocation)
                                      .Symbol as IMethodSymbol;
            if (methodSymbol == null ||
                methodSymbol.ContainingType != KnownSymbol.DependencyProperty)
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
            else if (methodSymbol.Name.StartsWith("Register", StringComparison.Ordinal))
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

            if (!field.ContainingType.IsSameType(ownerType))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, argument.GetLocation(), field, field.ContainingType));
            }
        }
    }
}