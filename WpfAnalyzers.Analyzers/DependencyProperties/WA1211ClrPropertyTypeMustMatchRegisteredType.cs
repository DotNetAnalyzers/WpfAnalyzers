namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WA1211ClrPropertyTypeMustMatchRegisteredType : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WA1211";
        private const string Title = "DependencyProperty CLR property type must match registered type.";
        private const string MessageFormat = "Property '{0}' must be of type {1}";
        private const string Description = Title;
        private const string HelpLink = "http://stackoverflow.com/";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
                                                                      DiagnosticId,
                                                                      Title,
                                                                      MessageFormat,
                                                                      AnalyzerCategory.DependencyProperties,
                                                                      DiagnosticSeverity.Error,
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

            TypeSyntax registeredType;
            if (!propertyDeclaration.TryGetDependencyPropertyRegisteredType(out registeredType))
            {
                return;
            }

            var propertyType = propertyDeclaration.Type;
            if (registeredType == null || propertyType == null)
            {
                return;
            }

            if (!context.SemanticModel.GetTypeInfo(propertyType).Equals(context.SemanticModel.GetTypeInfo(registeredType)))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, propertyType.GetLocation(), propertyDeclaration.Name(), context.SemanticModel.GetTypeInfo(registeredType).Type));
            }
        }
    }
}