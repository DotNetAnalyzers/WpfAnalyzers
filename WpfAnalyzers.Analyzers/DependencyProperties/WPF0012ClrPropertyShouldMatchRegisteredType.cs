namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0012ClrPropertyShouldMatchRegisteredType : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0012";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "CLR property type should match registered type.",
            messageFormat: "Property '{0}' must be of type {1}",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "CLR property type should match registered type.",
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

            var propertyDeclaration = context.Node as PropertyDeclarationSyntax;
            if (propertyDeclaration == null || propertyDeclaration.IsMissing)
            {
                return;
            }

            var property = context.ContainingSymbol as IPropertySymbol;
            if (property == null || !property.IsPotentialClrProperty())
            {
                return;
            }

            if (ClrProperty.TryGetRegisteredType(propertyDeclaration, context.SemanticModel, context.CancellationToken, out ITypeSymbol registeredType))
            {
                if (!registeredType.IsSameType(property.Type))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, propertyDeclaration.Type.GetLocation(), property, registeredType));
                }
            }
        }
    }
}