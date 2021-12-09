namespace WpfAnalyzers
{
    using System;
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
            Descriptors.WPF0024ParameterShouldBeNullable,
            Descriptors.WPF0150UseNameofInsteadOfLiteral,
            Descriptors.WPF0151UseNameofInsteadOfConstant);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(x => Handle(x), SyntaxKind.InvocationExpression);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is InvocationExpressionSyntax invocation &&
                context.ContainingSymbol is { IsStatic: true } &&
                DependencyProperty.Register.MatchAny(invocation, context.SemanticModel, context.CancellationToken) is { NameArgument: { } nameArgument } register &&
                register.PropertyName(context.SemanticModel, context.CancellationToken) is { } registeredName)
            {
                if (register.FindArgument(KnownSymbols.ValidateValueCallback) is { } validateValueCallback &&
                    Callback.Match(validateValueCallback, KnownSymbols.ValidateValueCallback, context.SemanticModel, context.CancellationToken) is { Identifier: { } identifier, Target: { } target })
                {
                    if (TypeSymbolComparer.Equal(target.ContainingType, context.ContainingSymbol.ContainingType) &&
                        !MatchesValidateValueCallbackName(validateValueCallback, target, context))
                    {
                        using var walker = InvocationWalker.InContainingClass(target, context.SemanticModel, context.CancellationToken);
                        if (walker.IdentifierNames.Count == 1)
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    Descriptors.WPF0007ValidateValueCallbackCallbackShouldMatchRegisteredName,
                                    identifier.GetLocation(),
                                    ImmutableDictionary<string, string?>.Empty.Add("ExpectedName", $"Validate{registeredName}"),
                                    identifier,
                                    $"Validate{registeredName}"));
                        }
                        else if (target.Name.StartsWith("Validate", StringComparison.Ordinal))
                        {
                            foreach (var identifierName in walker.IdentifierNames)
                            {
                                if (identifierName.TryFirstAncestor(out ArgumentSyntax? argument) &&
                                    argument != validateValueCallback &&
                                    MatchesValidateValueCallbackName(argument, target, context))
                                {
                                    context.ReportDiagnostic(
                                        Diagnostic.Create(
                                            Descriptors.WPF0007ValidateValueCallbackCallbackShouldMatchRegisteredName,
                                            identifier.GetLocation(),
                                            identifier,
                                            $"Validate{registeredName}"));
                                    break;
                                }
                            }
                        }
                    }

                    if (target.TrySingleMethodDeclaration(context.CancellationToken, out var declaration))
                    {
                        if (Callback.CanInlineBody(declaration))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0023ConvertToLambda, validateValueCallback.GetLocation()));
                        }

                        if (context.SemanticModel.GetNullableContext(declaration.SpanStart).HasFlag(NullableContext.Enabled) &&
                            declaration.ParameterList is { Parameters: { Count: 1 } parameters } &&
                            parameters[0] is { Type: { } type and not NullableTypeSyntax })
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0024ParameterShouldBeNullable, type.GetLocation()));
                        }
                    }
                }

                if (nameArgument.Expression is { } nameExpression &&
                    nameExpression.IsNameof() == false &&
                    context.ContainingSymbol.ContainingType.TryFindProperty(registeredName, out var property))
                {
                    if (nameArgument.Expression.IsKind(SyntaxKind.StringLiteralExpression))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.WPF0150UseNameofInsteadOfLiteral,
                                nameArgument.GetLocation(),
                                ImmutableDictionary<string, string?>.Empty.Add(nameof(IdentifierNameSyntax), property.Name),
                                property.Name));
                    }
                    else
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.WPF0151UseNameofInsteadOfConstant,
                                nameExpression.GetLocation(),
                                ImmutableDictionary<string, string?>.Empty.Add(nameof(IdentifierNameSyntax), property.Name),
                                property.Name));
                    }
                }
            }
        }

        private static bool MatchesValidateValueCallbackName(ArgumentSyntax validateValueCallback, IMethodSymbol target, SyntaxNodeAnalysisContext context)
        {
            return validateValueCallback.Parent is ArgumentListSyntax { Parent: InvocationExpressionSyntax invocation } &&
                   DependencyProperty.Register.MatchAny(invocation, context.SemanticModel, context.CancellationToken) is { } register &&
                   TypeSymbolComparer.Equal(target.ContainingType, context.ContainingSymbol?.ContainingType) &&
                   register.PropertyName(context.SemanticModel, context.CancellationToken) is { } registeredName &&
                   target.Name.IsParts("Validate", registeredName);
        }
    }
}
