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
                if (DependencyProperty.TryGetRegisterInvocationRecursive(fieldOrProperty, context.SemanticModel, context.CancellationToken, out var invocation))
                {
                    if (invocation.TryGetArgumentAtIndex(0, out var nameArg) &&
                        nameArg.TryGetStringValue(context.SemanticModel, context.CancellationToken, out var registeredName))
                    {
                        if (fieldOrProperty.Type == KnownSymbol.DependencyProperty &&
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
                else if (DependencyProperty.TryGetPropertyByName(fieldOrProperty, out var property))
                {
                    if (fieldOrProperty.Type == KnownSymbol.DependencyProperty &&
                        !fieldOrProperty.Name.IsParts(property.Name, "Property"))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                WPF0001BackingFieldShouldMatchRegisteredName.Descriptor,
                                fieldOrProperty.FindIdentifier(context.Node).GetLocation(),
                                fieldOrProperty.Name,
                                property.Name));
                    }

                    if (fieldOrProperty.Type == KnownSymbol.DependencyPropertyKey &&
                        !fieldOrProperty.Name.IsParts(property.Name, "PropertyKey"))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                WPF0002BackingFieldShouldMatchRegisteredName.Descriptor,
                                fieldOrProperty.FindIdentifier(context.Node).GetLocation(),
                                fieldOrProperty.Name,
                                property.Name));
                    }
                }
            }
        }
    }
}