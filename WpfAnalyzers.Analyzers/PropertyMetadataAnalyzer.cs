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
            WPF0006CoerceValueCallbackShouldMatchRegisteredName.Descriptor);

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
            }
        }
    }
}