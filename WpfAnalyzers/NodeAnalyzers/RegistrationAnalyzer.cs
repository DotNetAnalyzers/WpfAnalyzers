namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class RegistrationAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            WPF0007ValidateValueCallbackCallbackShouldMatchRegisteredName.Descriptor,
            WPF0023ConvertToLambda.Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(x => Handle(x), SyntaxKind.InvocationExpression);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.Node is InvocationExpressionSyntax registerCall &&
                context.ContainingSymbol.IsStatic &&
                (DependencyProperty.TryGetRegisterCall(registerCall, context.SemanticModel, context.CancellationToken, out var method) ||
                 DependencyProperty.TryGetRegisterReadOnlyCall(registerCall, context.SemanticModel, context.CancellationToken, out method) ||
                 DependencyProperty.TryGetRegisterAttachedCall(registerCall, context.SemanticModel, context.CancellationToken, out method) ||
                 DependencyProperty.TryGetRegisterAttachedReadOnlyCall(registerCall, context.SemanticModel, context.CancellationToken, out method)))
            {
                if (method.TryFindParameter(KnownSymbol.ValidateValueCallback, out var parameter) &&
                    registerCall.TryFindArgument(parameter, out var validateValueCallback) &&
                    Callback.TryGetTarget(validateValueCallback, KnownSymbol.ValidateValueCallback, context.SemanticModel, context.CancellationToken, out var callBackIdentifier, out var target))
                {
                    if (target.ContainingType.Equals(context.ContainingSymbol.ContainingType) &&
                        DependencyProperty.TryGetRegisteredName(registerCall, context.SemanticModel, context.CancellationToken, out var registeredName) &&
                        !target.Name.IsParts("Validate", registeredName) &&
                        target.IsInvokedOnce(context.SemanticModel, context.CancellationToken))
                    {
                        context.ReportDiagnostic(
                        Diagnostic.Create(
                            WPF0007ValidateValueCallbackCallbackShouldMatchRegisteredName.Descriptor,
                            callBackIdentifier.GetLocation(),
                            ImmutableDictionary<string, string>.Empty.Add("ExpectedName", $"Validate{registeredName}"),
                            callBackIdentifier,
                            $"Validate{registeredName}"));
                    }

                    if (target.TrySingleMethodDeclaration(context.CancellationToken, out var declaration) &&
                        Callback.IsSingleExpression(declaration))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(WPF0023ConvertToLambda.Descriptor, validateValueCallback.GetLocation()));
                    }
                }
            }
        }
    }
}
