namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WA1203ClrPropertyNameMustMatchRegisteredName : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WA1203";
        private const string Title = "DependencyProperty CLR property name must match registered name.";
        private const string MessageFormat = "Property '{0}' must be named {1}";
        private const string Description = Title;
        private const string HelpLink = "http://stackoverflow.com/";

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

            var registeredName = propertyDeclaration.DependencyPropertyRegisteredName();
            var propertyName = propertyDeclaration.Name();
            if (registeredName == null || propertyName == null)
            {
                return;
            }

            if (registeredName != propertyName)
            {
                var identifier = propertyDeclaration.Identifier;
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, identifier.GetLocation(), propertyName, registeredName));
            }
        }
    }
}