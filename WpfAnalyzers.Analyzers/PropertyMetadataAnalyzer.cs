namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class PropertyMetadataAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            WPF0005PropertyChangedCallbackShouldMatchRegisteredName.Descriptor,
            WPF0006CoerceValueCallbackShouldMatchRegisteredName.Descriptor,
            WPF0010DefaultValueMustMatchRegisteredType.Descriptor,
            WPF0016DefaultValueIsSharedReferenceType.Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Handle, SyntaxKind.ObjectCreationExpression);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.Node is ObjectCreationExpressionSyntax objectCreation &&
                context.ContainingSymbol.IsStatic &&
                PropertyMetadata.TryGetConstructor(objectCreation, context.SemanticModel, context.CancellationToken, out _))
            {
                if (PropertyMetadata.TryGetRegisteredName(objectCreation, context.SemanticModel, context.CancellationToken, out var registeredName))
                {
                    if (PropertyMetadata.TryGetPropertyChangedCallback(objectCreation, context.SemanticModel, context.CancellationToken, out var propertyChangedCallback) &&
                        PropertyChangedCallback.TryGetName(propertyChangedCallback, context.SemanticModel, context.CancellationToken, out var propertyChangedIdentifier, out _) &&
                        !propertyChangedIdentifier.Identifier.ValueText.IsParts("On", registeredName, "Changed"))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                WPF0005PropertyChangedCallbackShouldMatchRegisteredName.Descriptor,
                                propertyChangedIdentifier.GetLocation(),
                                ImmutableDictionary<string, string>.Empty.Add("ExpectedName", $"On{registeredName}Changed"),
                                propertyChangedIdentifier,
                                $"On{registeredName}Changed"));
                    }

                    if (PropertyMetadata.TryGetCoerceValueCallback(objectCreation, context.SemanticModel, context.CancellationToken, out var coerceValueCallback) &&
                        CoerceValueCallback.TryGetName(coerceValueCallback, context.SemanticModel, context.CancellationToken, out var coerceValueIdentifier, out _) &&
                        !coerceValueIdentifier.Identifier.ValueText.IsParts("Coerce", registeredName))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                WPF0006CoerceValueCallbackShouldMatchRegisteredName.Descriptor,
                                coerceValueIdentifier.GetLocation(),
                                ImmutableDictionary<string, string>.Empty.Add("ExpectedName", $"Coerce{registeredName}"),
                                coerceValueIdentifier,
                                $"Coerce{registeredName}"));
                    }
                }

                if (PropertyMetadata.TryGetDependencyProperty(objectCreation, context.SemanticModel, context.CancellationToken, out var fieldOrProperty) &&
                    DependencyProperty.TryGetRegisteredType(fieldOrProperty, context.SemanticModel, context.CancellationToken, out var registeredType) &&
                    PropertyMetadata.TryGetDefaultValue(objectCreation, context.SemanticModel, context.CancellationToken, out var defaultValueArg))
                {
                    if (!registeredType.IsRepresentationPreservingConversion(defaultValueArg.Expression, context.SemanticModel, context.CancellationToken) &&
                        !IsInvocationReturningObject(defaultValueArg.Expression, context))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                WPF0010DefaultValueMustMatchRegisteredType.Descriptor,
                                defaultValueArg.GetLocation(),
                                fieldOrProperty.Symbol,
                                registeredType));
                    }

                    if (registeredType.IsReferenceType)
                    {
                        var defaultValue = defaultValueArg.Expression;
                        if (defaultValue != null &&
                            !defaultValue.IsKind(SyntaxKind.NullLiteralExpression) &&
                            registeredType != KnownSymbol.FontFamily)
                        {
                            if (IsNonEmptyArrayCreation(defaultValue as ArrayCreationExpressionSyntax, context) ||
                                IsReferenceTypeCreation(defaultValue as ObjectCreationExpressionSyntax, context))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(WPF0016DefaultValueIsSharedReferenceType.Descriptor, defaultValueArg.GetLocation(), fieldOrProperty.Symbol));
                            }
                        }
                    }
                }
            }
        }

        private static bool IsInvocationReturningObject(ExpressionSyntax expression, SyntaxNodeAnalysisContext context)
        {
            return expression is InvocationExpressionSyntax invocation &&
                   context.SemanticModel.GetSymbolSafe(invocation, context.CancellationToken) is IMethodSymbol method &&
                   method.ReturnType == KnownSymbol.Object;
        }

        private static bool IsNonEmptyArrayCreation(ArrayCreationExpressionSyntax arrayCreation, SyntaxNodeAnalysisContext context)
        {
            if (arrayCreation == null)
            {
                return false;
            }

            foreach (var rank in arrayCreation.Type.RankSpecifiers)
            {
                foreach (var size in rank.Sizes)
                {
                    var constantValue = context.SemanticModel.GetConstantValueSafe(size, context.CancellationToken);
                    if (!constantValue.HasValue)
                    {
                        return true;
                    }

                    return !Equals(constantValue.Value, 0);
                }
            }

            return false;
        }

        private static bool IsReferenceTypeCreation(ObjectCreationExpressionSyntax objectCreation, SyntaxNodeAnalysisContext context)
        {
            if (objectCreation == null)
            {
                return false;
            }

            var type = context.SemanticModel.GetTypeInfoSafe(objectCreation, context.CancellationToken)
                              .Type;
            if (type.IsValueType)
            {
                return false;
            }

            return true;
        }
    }
}