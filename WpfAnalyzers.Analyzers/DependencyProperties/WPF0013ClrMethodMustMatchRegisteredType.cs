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
            var methodDeclaration = context.Node as MethodDeclarationSyntax;
            if (methodDeclaration == null || methodDeclaration.IsMissing)
            {
                return;
            }

            var method = context.ContainingSymbol as IMethodSymbol;
            if (method == null)
            {
                return;
            }

            IFieldSymbol getField;
            if (ClrMethod.IsAttachedGetMethod(method, context.SemanticModel, context.CancellationToken, out getField))
            {
                ITypeSymbol registeredType;
                if (DependencyProperty.TryGetRegisteredType(getField, context.SemanticModel, context.CancellationToken, out registeredType))
                {
                    if (!method.ReturnType.IsSameType(registeredType))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, methodDeclaration.ReturnType.GetLocation(), "Return type", registeredType));
                    }
                }

                return;
            }

            IFieldSymbol setField;
            if (ClrMethod.IsAttachedSetMethod(method, context.SemanticModel, context.CancellationToken, out setField))
            {
                ITypeSymbol registeredType;
                if (DependencyProperty.TryGetRegisteredType(
                    setField,
                    context.SemanticModel,
                    context.CancellationToken,
                    out registeredType))
                {
                    if (!method.Parameters[1].Type.IsSameType(registeredType))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptor,
                                methodDeclaration.ParameterList.Parameters[1].GetLocation(),
                                "Value type",
                                registeredType));
                    }
                }
            }
        }
    }
}