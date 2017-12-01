namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class DependencyPropertyBackingFieldOrPropertyAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            WPF0001BackingFieldShouldMatchRegisteredName.Descriptor,
            WPF0002BackingFieldShouldMatchRegisteredName.Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Handle, SyntaxKind.FieldDeclaration, SyntaxKind.PropertyDeclaration);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (BackingFieldOrProperty.TryCreate(context.ContainingSymbol, out var fieldOrProperty))
            {
                if (fieldOrProperty.Type == KnownSymbol.DependencyProperty &&
                    DependencyProperty.TryGetRegisteredName(
                        fieldOrProperty,
                        context.SemanticModel,
                        context.CancellationToken,
                        out var registeredName) &&
                    !fieldOrProperty.Name.IsParts(registeredName, "Property"))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            WPF0001BackingFieldShouldMatchRegisteredName.Descriptor,
                            fieldOrProperty.FindIdentifier(context.Node).GetLocation(),
                            fieldOrProperty.Name,
                            registeredName));
                }

                if (fieldOrProperty.Type == KnownSymbol.DependencyPropertyKey &&
                    DependencyProperty.TryGetRegisteredName(fieldOrProperty, context.SemanticModel, context.CancellationToken, out registeredName) &&
                    !fieldOrProperty.Name.IsParts(registeredName, "PropertyKey"))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            WPF0002BackingFieldShouldMatchRegisteredName.Descriptor,
                            fieldOrProperty.FindIdentifier(context.Node).GetLocation(),
                            fieldOrProperty.Name,
                            registeredName));
                }
            }
        }
    }
}