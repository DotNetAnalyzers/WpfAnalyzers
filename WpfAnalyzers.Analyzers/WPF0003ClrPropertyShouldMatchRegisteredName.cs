namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0003ClrPropertyShouldMatchRegisteredName : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0003";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "CLR property for a DependencyProperty should match registered name.",
            messageFormat: "Property '{0}' must be named '{1}'",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "A CLR property accessor for a DependencyProperty must have the same name as the DependencyProperty is registered with.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Handle, SyntaxKind.PropertyDeclaration);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.Node is PropertyDeclarationSyntax propertyDeclaration &&
                context.ContainingSymbol is IPropertySymbol property &&
                property.ContainingType.Is(KnownSymbol.DependencyObject) &&
                ClrProperty.TryGetRegisteredName(propertyDeclaration, context.SemanticModel, context.CancellationToken, out var registeredName) &&
                registeredName != property.Name)
            {
                var identifier = propertyDeclaration.Identifier;
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptor,
                        identifier.GetLocation(),
                        property.Name,
                        registeredName));
            }
        }
    }
}