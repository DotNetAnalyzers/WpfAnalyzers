namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class SetValueAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            WPF0014SetValueMustUseRegisteredType.Descriptor,
            WPF0040SetUsingDependencyPropertyKey.Descriptor,
            WPF0043DontUseSetCurrentValueForDataContext.Descriptor);

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

            if (context.Node is InvocationExpressionSyntax invocation &&
                (DependencyObject.TryGetSetValueCall(invocation, context.SemanticModel, context.CancellationToken, out var call) ||
                 DependencyObject.TryGetSetCurrentValueCall(invocation, context.SemanticModel, context.CancellationToken, out call)) &&
                 invocation.TryGetArgumentAtIndex(0, out var propertyArg) &&
                 invocation.TryGetArgumentAtIndex(1, out var valueArg))
            {
                var property = context.SemanticModel.GetSymbolSafe(propertyArg.Expression, context.CancellationToken);
                if (BackingFieldOrProperty.TryCreate(property, out var fieldOrProperty))
                {
                    if (!valueArg.Expression.IsSameType(KnownSymbol.Object, context))
                    {
                        if (DependencyProperty.TryGetRegisteredType(fieldOrProperty, context.SemanticModel, context.CancellationToken, out var registeredType))
                        {
                            if (registeredType.Is(KnownSymbol.Freezable) &&
                                valueArg.Expression.IsSameType(KnownSymbol.Freezable, context))
                            {
                                return;
                            }

                            if (!context.SemanticModel.IsRepresentationPreservingConversion(valueArg.Expression, registeredType, context.CancellationToken))
                            {
                                var setCall = context.SemanticModel.GetSymbolSafe(invocation, context.CancellationToken);
                                context.ReportDiagnostic(Diagnostic.Create(WPF0014SetValueMustUseRegisteredType.Descriptor, valueArg.GetLocation(), setCall.Name, registeredType));
                            }
                        }
                    }

                    if (fieldOrProperty.Type == KnownSymbol.DependencyProperty)
                    {
                        if (DependencyProperty.TryGetDependencyPropertyKeyField(fieldOrProperty, context.SemanticModel, context.CancellationToken, out var keyField))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    WPF0040SetUsingDependencyPropertyKey.Descriptor,
                                    propertyArg.GetLocation(),
                                    propertyArg,
                                    keyField.CreateArgument(context.SemanticModel, propertyArg.SpanStart)));
                        }
                    }
                }

                if (call == KnownSymbol.DependencyObject.SetCurrentValue &&
                    property is IFieldSymbol setField &&
                    setField == KnownSymbol.FrameworkElement.DataContextProperty)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            WPF0043DontUseSetCurrentValueForDataContext.Descriptor,
                            invocation.GetLocation(),
                            setField.Name,
                            invocation.ArgumentList.Arguments[1]));
                }
            }
        }
    }
}
