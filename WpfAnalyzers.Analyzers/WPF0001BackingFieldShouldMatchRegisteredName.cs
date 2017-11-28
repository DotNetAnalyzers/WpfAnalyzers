namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0001BackingFieldShouldMatchRegisteredName : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0001";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Backing field for a DependencyProperty should match registered name.",
            messageFormat: "Field '{0}' that is backing field for the DependencyProperty registered as '{1}' must be named '{1}Property'",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A dependency property's backing field must be named with the name it is registered with suffixed by 'Property'",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Descriptor);

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

            if (BackingFieldOrProperty.TryCreate(context.ContainingSymbol, out var fieldOrProperty) &&
                fieldOrProperty.Type == KnownSymbol.DependencyProperty &&
                DependencyProperty.TryGetRegisteredName(fieldOrProperty, context.SemanticModel, context.CancellationToken, out var registeredName) &&
                !fieldOrProperty.Name.IsParts(registeredName, "Property"))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptor,
                        fieldOrProperty.FindIdentifier(context.Node).GetLocation(),
                        fieldOrProperty.Name,
                        registeredName));
            }
        }
    }
}
