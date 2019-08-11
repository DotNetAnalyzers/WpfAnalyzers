namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class PropertyMetadataAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Descriptors.WPF0005PropertyChangedCallbackShouldMatchRegisteredName,
            Descriptors.WPF0006CoerceValueCallbackShouldMatchRegisteredName,
            Descriptors.WPF0010DefaultValueMustMatchRegisteredType,
            Descriptors.WPF0016DefaultValueIsSharedReferenceType,
            Descriptors.WPF0023ConvertToLambda);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(c => Handle(c), SyntaxKind.ObjectCreationExpression);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is ObjectCreationExpressionSyntax objectCreation &&
                context.ContainingSymbol.IsStatic &&
                PropertyMetadata.TryGetConstructor(objectCreation, context.SemanticModel, context.CancellationToken, out _))
            {
                if (PropertyMetadata.TryGetRegisteredName(objectCreation, context.SemanticModel, context.CancellationToken, out var registeredName))
                {
                    if (PropertyMetadata.TryGetPropertyChangedCallback(objectCreation, context.SemanticModel, context.CancellationToken, out var propertyChangedCallback) &&
                        Callback.TryGetTarget(propertyChangedCallback, KnownSymbol.PropertyChangedCallback, context.SemanticModel, context.CancellationToken, out var callbackIdentifier, out var target))
                    {
                        if (target.ContainingType.Equals(context.ContainingSymbol.ContainingType) &&
                            !target.Name.IsParts("On", registeredName, "Changed") &&
                            target.DeclaredAccessibility.IsEither(Accessibility.Private, Accessibility.Protected))
                        {
                            using (var walker = InvocationWalker.InContainingClass(target, context.SemanticModel, context.CancellationToken))
                            {
                                if (walker.IdentifierNames.Count == 1)
                                {
                                    context.ReportDiagnostic(
                                        Diagnostic.Create(
                                            Descriptors.WPF0005PropertyChangedCallbackShouldMatchRegisteredName,
                                            callbackIdentifier.GetLocation(),
                                            ImmutableDictionary<string, string>.Empty.Add("ExpectedName", $"On{registeredName}Changed"),
                                            callbackIdentifier,
                                            $"On{registeredName}Changed"));
                                }
                                else if (target.Name.StartsWith("On") &&
                                         target.Name.EndsWith("Changed"))
                                {
                                    foreach (var identifierName in walker.IdentifierNames)
                                    {
                                        if (identifierName.TryFirstAncestor(out ArgumentSyntax argument) &&
                                            argument != propertyChangedCallback &&
                                            MatchesPropertyChangedCallbackName(argument, target, context))
                                        {
                                            context.ReportDiagnostic(
                                                Diagnostic.Create(
                                                    Descriptors.WPF0005PropertyChangedCallbackShouldMatchRegisteredName,
                                                    callbackIdentifier.GetLocation(),
                                                    callbackIdentifier,
                                                    $"On{registeredName}Changed"));
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        if (target.TrySingleMethodDeclaration(context.CancellationToken, out var declaration) &&
                            Callback.IsSingleExpression(declaration))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0023ConvertToLambda, propertyChangedCallback.GetLocation()));
                        }
                    }

                    if (PropertyMetadata.TryGetCoerceValueCallback(objectCreation, context.SemanticModel, context.CancellationToken, out var coerceValueCallback) &&
                        Callback.TryGetTarget(coerceValueCallback, KnownSymbol.CoerceValueCallback, context.SemanticModel, context.CancellationToken, out callbackIdentifier, out target))
                    {
                        if (target.ContainingType.Equals(context.ContainingSymbol.ContainingType) &&
                            !target.Name.IsParts("Coerce", registeredName))
                        {
                            using (var walker = InvocationWalker.InContainingClass(target, context.SemanticModel, context.CancellationToken))
                            {
                                if (walker.IdentifierNames.Count == 1)
                                {
                                    context.ReportDiagnostic(
                                        Diagnostic.Create(
                                            Descriptors.WPF0006CoerceValueCallbackShouldMatchRegisteredName,
                                            callbackIdentifier.GetLocation(),
                                            ImmutableDictionary<string, string>.Empty.Add("ExpectedName", $"Coerce{registeredName}"),
                                            callbackIdentifier,
                                            $"Coerce{registeredName}"));
                                }
                                else if (target.Name.StartsWith("Coerce"))
                                {
                                    foreach (var identifierName in walker.IdentifierNames)
                                    {
                                        if (identifierName.TryFirstAncestor(out ArgumentSyntax argument) &&
                                            argument != propertyChangedCallback &&
                                            MatchesCoerceValueCallbackName(argument, target, context))
                                        {
                                            context.ReportDiagnostic(
                                                Diagnostic.Create(
                                                    Descriptors.WPF0006CoerceValueCallbackShouldMatchRegisteredName,
                                                    callbackIdentifier.GetLocation(),
                                                    callbackIdentifier,
                                                    $"Coerce{registeredName}"));
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        if (target.TrySingleMethodDeclaration(context.CancellationToken, out var declaration) &&
                            Callback.IsSingleExpression(declaration))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0023ConvertToLambda, propertyChangedCallback.GetLocation()));
                        }
                    }
                }

                if (PropertyMetadata.TryGetDependencyProperty(objectCreation, context.SemanticModel, context.CancellationToken, out var fieldOrProperty) &&
                    DependencyProperty.TryGetRegisteredType(fieldOrProperty, context.SemanticModel, context.CancellationToken, out var registeredType) &&
                    PropertyMetadata.TryGetDefaultValue(objectCreation, context.SemanticModel, context.CancellationToken, out var defaultValueArg))
                {
                    if (!PropertyMetadata.IsValueValidForRegisteredType(defaultValueArg.Expression, registeredType, context.SemanticModel, context.CancellationToken))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.WPF0010DefaultValueMustMatchRegisteredType,
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
                                context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0016DefaultValueIsSharedReferenceType, defaultValueArg.GetLocation(), fieldOrProperty.Symbol));
                            }
                        }
                    }
                }
            }
        }

        private static bool MatchesPropertyChangedCallbackName(ArgumentSyntax propertyChangedCallback, IMethodSymbol target, SyntaxNodeAnalysisContext context)
        {
            return propertyChangedCallback.Parent is ArgumentListSyntax argumentList &&
                   argumentList.Parent is ObjectCreationExpressionSyntax objectCreation &&
                   PropertyMetadata.TryGetRegisteredName(objectCreation, context.SemanticModel, context.CancellationToken, out var registeredName) &&
                   target.ContainingType.Equals(context.ContainingSymbol.ContainingType) &&
                   target.Name.IsParts("On", registeredName, "Changed");
        }

        private static bool MatchesCoerceValueCallbackName(ArgumentSyntax coerceValueCallback, IMethodSymbol target, SyntaxNodeAnalysisContext context)
        {
            return coerceValueCallback.Parent is ArgumentListSyntax argumentList &&
                   argumentList.Parent is ObjectCreationExpressionSyntax objectCreation &&
                   PropertyMetadata.TryGetRegisteredName(objectCreation, context.SemanticModel, context.CancellationToken, out var registeredName) &&
                   target.ContainingType.Equals(context.ContainingSymbol.ContainingType) &&
                   target.Name.IsParts("Coerce", registeredName);
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
