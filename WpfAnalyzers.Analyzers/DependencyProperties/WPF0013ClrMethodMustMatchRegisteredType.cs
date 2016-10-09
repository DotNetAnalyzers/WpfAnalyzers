namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0013ClrMethodMustMatchRegisteredType : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0013";
        private const string Title = "CLR accessor for attached property must match registered type.";
        private const string MessageFormat = "{0} must match registered type {1}";
        private const string Description = Title;
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
            context.RegisterSyntaxNodeAction(HandleDeclaration, SyntaxKind.MethodDeclaration);
        }

        private static void HandleDeclaration(SyntaxNodeAnalysisContext context)
        {
            var method = context.Node as MethodDeclarationSyntax;
            if (method == null || method.IsMissing)
            {
                return;
            }

            var methodSymbol = context.ContainingSymbol as IMethodSymbol;
            if (methodSymbol == null)
            {
                return;
            }

            ITypeSymbol registeredType;
            if (method.TryGetDependencyPropertyRegisteredTypeFromAttachedGet(context.SemanticModel, context.CancellationToken, out registeredType))
            {
                if (!methodSymbol.ReturnType.IsSameType(registeredType))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, method.ReturnType.GetLocation(), "Return type", registeredType));
                }
            }
            else if (method.TryGetDependencyPropertyRegisteredTypeFromAttachedSet(context.SemanticModel, context.CancellationToken, out registeredType))
            {
                if (!methodSymbol.Parameters[1].Type.IsSameType(registeredType))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, method.ParameterList.Parameters[1].GetLocation(), "Value type", registeredType));
                }
            }
        }
    }
}