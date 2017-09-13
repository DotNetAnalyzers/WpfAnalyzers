namespace WpfAnalyzers.PropertyChanged
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using WpfAnalyzers.PropertyChanged.Helpers;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF1010MutablePublicPropertyShouldNotify : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF1010";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Mutable public property should notify.",
            messageFormat: "Property '{0}' must notify when value changes.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "All mutable public properties should notify when their value changes.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(HandleDeclaration, SyntaxKind.PropertyDeclaration);
        }

        private static void HandleDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            var propertySymbol = (IPropertySymbol)context.ContainingSymbol;
            if (!propertySymbol.ContainingType.Is(KnownSymbol.INotifyPropertyChanged))
            {
                return;
            }

            var declaration = (PropertyDeclarationSyntax)context.Node;
            if (Property.ShouldNotify(declaration, propertySymbol, context.SemanticModel, context.CancellationToken))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, declaration.GetLocation(), context.ContainingSymbol.Name));
            }
        }
    }
}