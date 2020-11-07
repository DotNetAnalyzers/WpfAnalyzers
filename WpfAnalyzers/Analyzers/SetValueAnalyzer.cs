namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;

    using Gu.Roslyn.AnalyzerExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class SetValueAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Descriptors.WPF0014SetValueMustUseRegisteredType,
            Descriptors.WPF0040SetUsingDependencyPropertyKey,
            Descriptors.WPF0043DoNotUseSetCurrentValueForDataContext);

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
                context.SemanticModel.TryGetSymbol(propertyArg.Expression, context.CancellationToken, out var symbol) &&
                BackingFieldOrProperty.Match(symbol) is { } backing)
            {
                if (IsWrongType(backing, valueArg, context) is { } registeredType)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptors.WPF0014SetValueMustUseRegisteredType,
                            valueArg.GetLocation(),
                            target.Name,
                            registeredType));
                }

                if (backing.Type == KnownSymbols.DependencyProperty &&
                    backing.FindKey(context.SemanticModel, context.CancellationToken) is { } keyField)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptors.WPF0040SetUsingDependencyPropertyKey,
                            propertyArg.GetLocation(),
                            properties: ImmutableDictionary<string, string?>.Empty.Add(nameof(DependencyPropertyKeyType), keyField.Name),
                            propertyArg,
                            keyField.CreateArgument(context.SemanticModel, propertyArg.SpanStart)));
                }

                if (target == KnownSymbols.DependencyObject.SetCurrentValue &&
                    backing.Symbol is IFieldSymbol setField &&
                    setField == KnownSymbols.FrameworkElement.DataContextProperty)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptors.WPF0043DoNotUseSetCurrentValueForDataContext,
                            invocation.GetLocation(),
                            setField.Name,
                            invocation.ArgumentList.Arguments[1]));
                }
            }
        }

        private static ITypeSymbol? IsWrongType(BackingFieldOrProperty fieldOrProperty, ArgumentSyntax argument, SyntaxNodeAnalysisContext context)
        {
            if (fieldOrProperty.RegisteredType(context.SemanticModel, context.CancellationToken) is { Value: { } registeredType } &&
                !PropertyMetadata.IsValueValidForRegisteredType(argument.Expression, registeredType, context.SemanticModel, context.CancellationToken))
            {
                if (context.SemanticModel.TryGetType(argument.Expression, context.CancellationToken, out var type))
                {
                    if (type == KnownSymbols.Object)
                    {
                        return null;
                    }

                    if (registeredType.IsAssignableTo(KnownSymbols.Freezable, context.SemanticModel.Compilation) &&
                        type.IsAssignableTo(KnownSymbols.Freezable, context.SemanticModel.Compilation))
                    {
                        return null;
                    }
                }

                return registeredType;
            }

            return null;
        }

        private static bool TryGetArgs(SyntaxNodeAnalysisContext context, [NotNullWhen(true)] out IMethodSymbol? target, [NotNullWhen(true)] out ArgumentSyntax? propertyArg, [NotNullWhen(true)] out ArgumentSyntax? valueArg)
        {
            if (context.Node is InvocationExpressionSyntax invocation)
            {
                var propertyParameter = QualifiedParameter.Create(KnownSymbols.DependencyProperty);
                var valueParameter = QualifiedParameter.Create(KnownSymbols.Object);
                return invocation.TryGetTarget(KnownSymbols.DependencyObject.SetValue, propertyParameter, valueParameter, context.SemanticModel, context.CancellationToken, out target, out propertyArg, out valueArg) ||
                       invocation.TryGetTarget(KnownSymbols.DependencyObject.SetValue, QualifiedParameter.Create(KnownSymbols.DependencyPropertyKey), valueParameter, context.SemanticModel, context.CancellationToken, out target, out propertyArg, out valueArg) ||
                       invocation.TryGetTarget(KnownSymbols.DependencyObject.SetCurrentValue, propertyParameter, valueParameter, context.SemanticModel, context.CancellationToken, out target, out propertyArg, out valueArg);
            }

            target = null;
            propertyArg = null;
            valueArg = null;
            return false;
        }
    }
}
