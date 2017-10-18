﻿namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0014SetValueMustUseRegisteredType : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0014";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "SetValue must use registered type.",
            messageFormat: "{0} must use registered type {1}",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "Use a type that matches registered type when setting the value of a DependencyProperty",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

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
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            var invocation = context.Node as InvocationExpressionSyntax;
            if (invocation == null || context.SemanticModel == null)
            {
                return;
            }

            if (DependencyObject.TryGetSetValueArguments(invocation, context.SemanticModel, context.CancellationToken, out ArgumentSyntax property, out IFieldSymbol setField, out ArgumentSyntax value) ||
DependencyObject.TryGetSetCurrentValueArguments(invocation, context.SemanticModel, context.CancellationToken, out property, out setField, out value))
            {
                if (value.Expression.IsSameType(KnownSymbol.Object, context))
                {
                    return;
                }

                if (DependencyProperty.TryGetRegisteredType(setField, context.SemanticModel, context.CancellationToken, out ITypeSymbol registeredType))
                {
                    if (registeredType.Is(KnownSymbol.Freezable) &&
                        value.Expression.IsSameType(KnownSymbol.Freezable, context))
                    {
                        return;
                    }

                    if (!registeredType.IsRepresentationPreservingConversion(value.Expression, context.SemanticModel, context.CancellationToken))
                    {
                        var setCall = context.SemanticModel.GetSymbolSafe(invocation, context.CancellationToken);
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, value.GetLocation(), setCall.Name, registeredType));
                    }
                }
            }
        }
    }
}