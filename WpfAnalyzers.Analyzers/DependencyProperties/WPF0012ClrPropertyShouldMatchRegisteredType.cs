namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using WpfAnalyzers.SymbolHelpers;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0012ClrPropertyShouldMatchRegisteredType : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0012";
        private const string Title = "CLR property type should match registered type.";
        private const string MessageFormat = "Property '{0}' must be of type {1}";
        private const string Description = Title;
        private static readonly string HelpLink = WpfAnalyzers.HelpLink.ForId(DiagnosticId);

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
                                                                      DiagnosticId,
                                                                      Title,
                                                                      MessageFormat,
                                                                      AnalyzerCategory.DependencyProperties,
                                                                      DiagnosticSeverity.Warning,
                                                                      AnalyzerConstants.EnabledByDefault,
                                                                      Description,
                                                                      HelpLink);

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
            var propertyDeclaration = context.Node as PropertyDeclarationSyntax;
            if (propertyDeclaration == null || propertyDeclaration.IsMissing)
            {
                return;
            }

            var propertySymbol = context.ContainingSymbol as IPropertySymbol;
            if (propertySymbol == null)
            {
                return;
            }

            ITypeSymbol registeredType;
            if (!propertyDeclaration.TryGetDependencyPropertyRegisteredType(context.SemanticModel, context.CancellationToken, out registeredType))
            {
                return;
            }

            var actualTypeSymbol = propertySymbol.Type;
            if (!actualTypeSymbol.IsSameType(registeredType))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, propertyDeclaration.Type.GetLocation(), propertySymbol, registeredType));
            }
        }
    }
}