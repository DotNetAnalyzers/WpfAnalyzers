namespace WpfAnalyzers
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

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "CLR accessor for attached property must match registered type.",
            messageFormat: "{0} must match registered type {1}",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "CLR accessor for attached property must match registered type.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

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
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.Node is MethodDeclarationSyntax methodDeclaration &&
                context.ContainingSymbol is IMethodSymbol method)
            {
                if (ClrMethod.IsAttachedGetMethod(method, context.SemanticModel, context.CancellationToken, out var getField))
                {
                    if (DependencyProperty.TryGetRegisteredType(getField, context.SemanticModel, context.CancellationToken, out var registeredType))
                    {
                        if (!method.ReturnType.IsSameType(registeredType))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptor, methodDeclaration.ReturnType.GetLocation(), "Return type", registeredType));
                        }
                    }

                    return;
                }

                if (ClrMethod.IsAttachedSetMethod(
                    method,
                    context.SemanticModel,
                    context.CancellationToken,
                    out var setField))
                {
                    if (DependencyProperty.TryGetRegisteredType(
                        setField,
                        context.SemanticModel,
                        context.CancellationToken,
                        out var registeredType))
                    {
                        if (!method.Parameters[1]
                                   .Type.IsSameType(registeredType))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    Descriptor,
                                    methodDeclaration.ParameterList.Parameters[1]
                                                     .GetLocation(),
                                    "Value type",
                                    registeredType));
                        }
                    }
                }
            }
        }
    }
}