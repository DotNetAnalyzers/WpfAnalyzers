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
            context.RegisterSyntaxNodeAction(x => Handle(x), SyntaxKind.InvocationExpression);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is InvocationExpressionSyntax invocation &&
                TryGetArgs(context, out var target, out var propertyArg, out var valueArg) &&
                context.SemanticModel.TryGetSymbol(propertyArg.Expression, context.CancellationToken, out ISymbol symbol) &&
                BackingFieldOrProperty.TryCreateForDependencyProperty(symbol, out var fieldOrProperty))
            {
                if (IsWrongType(fieldOrProperty, valueArg, context, out ITypeSymbol registeredType))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            WPF0014SetValueMustUseRegisteredType.Descriptor,
                            valueArg.GetLocation(),
                            target.Name,
                            registeredType));
                }

                if (fieldOrProperty.Type == KnownSymbol.DependencyProperty &&
                    DependencyProperty.TryGetDependencyPropertyKeyField(fieldOrProperty, context.SemanticModel, context.CancellationToken, out var keyField))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            WPF0040SetUsingDependencyPropertyKey.Descriptor,
                            propertyArg.GetLocation(),
                            propertyArg,
                            keyField.CreateArgument(context.SemanticModel, propertyArg.SpanStart)));
                }

                if (target == KnownSymbol.DependencyObject.SetCurrentValue &&
                    fieldOrProperty.Symbol is IFieldSymbol setField &&
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

        private static bool IsWrongType(BackingFieldOrProperty fieldOrProperty, ArgumentSyntax argument, SyntaxNodeAnalysisContext context, out ITypeSymbol registeredType)
        {
            if (DependencyProperty.TryGetRegisteredType(fieldOrProperty, context.SemanticModel, context.CancellationToken, out registeredType) &&
                !context.SemanticModel.IsRepresentationPreservingConversion(argument.Expression, registeredType, context.CancellationToken))
            {
                if (context.SemanticModel.TryGetType(argument.Expression, context.CancellationToken, out var type))
                {
                    if (type == KnownSymbol.Object)
                    {
                        return false;
                    }

                    if (registeredType.IsAssignableTo(KnownSymbol.Freezable, context.Compilation) &&
                        type.IsAssignableTo(KnownSymbol.Freezable, context.Compilation))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        private static bool TryGetArgs(SyntaxNodeAnalysisContext context, out IMethodSymbol target, out ArgumentSyntax propertyArg, out ArgumentSyntax valueArg)
        {
            if (context.Node is InvocationExpressionSyntax invocation)
            {
                var propertyParameter = QualifiedParameter.Create(KnownSymbol.DependencyProperty);
                var valueParameter = QualifiedParameter.Create(KnownSymbol.Object);
                return invocation.TryGetTarget(KnownSymbol.DependencyObject.SetValue, propertyParameter, valueParameter, context.SemanticModel, context.CancellationToken, out target, out propertyArg, out valueArg) ||
                       invocation.TryGetTarget(KnownSymbol.DependencyObject.SetValue, QualifiedParameter.Create(KnownSymbol.DependencyPropertyKey), valueParameter, context.SemanticModel, context.CancellationToken, out target, out propertyArg, out valueArg) ||
                       invocation.TryGetTarget(KnownSymbol.DependencyObject.SetCurrentValue, propertyParameter, valueParameter, context.SemanticModel, context.CancellationToken, out target, out propertyArg, out valueArg);
            }

            target = null;
            propertyArg = null;
            valueArg = null;
            return false;
        }
    }
}
