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
            Descriptors.WPF0007ValidateValueCallbackCallbackShouldMatchRegisteredName,
            Descriptors.WPF0023ConvertToLambda,
            Descriptors.WPF0150UseNameof);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(x => Handle(x), SyntaxKind.InvocationExpression);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is InvocationExpressionSyntax registerCall &&
                context.ContainingSymbol.IsStatic &&
                TryGetAnyRegisterCall(registerCall, context, out var registerMethod) &&
                DependencyProperty.TryGetRegisteredName(registerCall, context.SemanticModel, context.CancellationToken, out var nameArg, out var registeredName))
            {
                if (registerMethod.TryFindParameter(KnownSymbol.ValidateValueCallback, out var parameter) &&
                    registerCall.TryFindArgument(parameter, out var validateValueCallback) &&
                    Callback.TryGetTarget(validateValueCallback, KnownSymbol.ValidateValueCallback, context.SemanticModel, context.CancellationToken, out var callBackIdentifier, out var target))
                {
                    if (target.ContainingType.Equals(context.ContainingSymbol.ContainingType) &&
                        !MatchesValidateValueCallbackName(validateValueCallback, target, context))
                    {
                        using (var walker = InvocationWalker.InContainingClass(target, context.SemanticModel, context.CancellationToken))
                        {
                            if (walker.IdentifierNames.Count == 1)
                            {
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        Descriptors.WPF0007ValidateValueCallbackCallbackShouldMatchRegisteredName,
                                        callBackIdentifier.GetLocation(),
                                        ImmutableDictionary<string, string>.Empty.Add("ExpectedName", $"Validate{registeredName}"),
                                        callBackIdentifier,
                                        $"Validate{registeredName}"));
                            }
                            else if (target.Name.StartsWith("Validate"))
                            {
                                foreach (var identifierName in walker.IdentifierNames)
                                {
                                    if (identifierName.TryFirstAncestor(out ArgumentSyntax argument) &&
                                        argument != validateValueCallback &&
                                        MatchesValidateValueCallbackName(argument, target, context))
                                    {
                                        context.ReportDiagnostic(
                                            Diagnostic.Create(
                                                Descriptors.WPF0007ValidateValueCallbackCallbackShouldMatchRegisteredName,
                                                callBackIdentifier.GetLocation(),
                                                callBackIdentifier,
                                                $"Validate{registeredName}"));
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (target.TrySingleMethodDeclaration(context.CancellationToken, out var declaration) &&
                        Callback.IsSingleExpression(declaration))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0023ConvertToLambda, validateValueCallback.GetLocation()));
                    }
                }

                if (nameArg?.Expression is LiteralExpressionSyntax &&
                    context.ContainingSymbol.ContainingType.TryFindProperty(registeredName, out var property))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptors.WPF0150UseNameof,
                            nameArg.GetLocation(),
                            ImmutableDictionary<string, string>.Empty.Add(nameof(IdentifierNameSyntax), property.Name),
                            property.Name));
                }
            }
        }

        private static bool MatchesValidateValueCallbackName(ArgumentSyntax validateValueCallback, IMethodSymbol target, SyntaxNodeAnalysisContext context)
        {
            return validateValueCallback.Parent is ArgumentListSyntax argumentList &&
                   argumentList.Parent is InvocationExpressionSyntax invocation &&
                   TryGetAnyRegisterCall(invocation, context, out _) &&
                   target.ContainingType.Equals(context.ContainingSymbol.ContainingType) &&
                   DependencyProperty.TryGetRegisteredName(invocation, context.SemanticModel, context.CancellationToken, out _, out var registeredName) &&
                   target.Name.IsParts("Validate", registeredName);
        }

        private static bool TryGetAnyRegisterCall(InvocationExpressionSyntax registerCall, SyntaxNodeAnalysisContext context, out IMethodSymbol method)
        {
            return DependencyProperty.TryGetRegisterCall(registerCall, context.SemanticModel, context.CancellationToken, out method) ||
                   DependencyProperty.TryGetRegisterReadOnlyCall(registerCall, context.SemanticModel, context.CancellationToken, out method) ||
                   DependencyProperty.TryGetRegisterAttachedCall(registerCall, context.SemanticModel, context.CancellationToken, out method) ||
                   DependencyProperty.TryGetRegisterAttachedReadOnlyCall(registerCall, context.SemanticModel, context.CancellationToken, out method);
        }
    }
}
