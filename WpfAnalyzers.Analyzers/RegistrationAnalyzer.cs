namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class RegistrationAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            WPF0007ValidateValueCallbackCallbackShouldMatchRegisteredName.Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Handle, SyntaxKind.InvocationExpression);
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
                if (Argument.TryGetArgument(method.Parameters, registerCall.ArgumentList, KnownSymbol.ValidateValueCallback, out var validateValueCallback) &&
                    Callback.TryGetName(validateValueCallback, KnownSymbol.ValidateValueCallback, context.SemanticModel, context.CancellationToken, out var callBackIdentifier, out _) &&
                    DependencyProperty.TryGetRegisteredName(registerCall, context.SemanticModel, context.CancellationToken, out var registeredName) &&
                    !callBackIdentifier.Identifier.ValueText.IsParts("Validate", registeredName))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            WPF0007ValidateValueCallbackCallbackShouldMatchRegisteredName.Descriptor,
                            callBackIdentifier.GetLocation(),
                            ImmutableDictionary<string, string>.Empty.Add("ExpectedName", $"Validate{registeredName}"),
                            callBackIdentifier,
                            $"Validate{registeredName}"));
                }
            }
        }
    }
}