namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class RoutedEventAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            WPF0100BackingFieldShouldMatchRegisteredName.Descriptor);

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

            if (FieldOrProperty.TryCreate(context.ContainingSymbol, out var fieldOrProperty) &&
                fieldOrProperty.Type == KnownSymbol.RoutedEvent &&
                RoutedEvent.TryGetRegisteredName(fieldOrProperty, context.SemanticModel, context.CancellationToken, out var registeredName))
            {
                if (!fieldOrProperty.Name.IsParts(registeredName, "Event"))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            WPF0100BackingFieldShouldMatchRegisteredName.Descriptor,
                            fieldOrProperty.FindIdentifier(context.Node).GetLocation(),
                            fieldOrProperty.Name,
                            registeredName));
                }
            }
        }
    }
}