namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WA1212ClrPropertyTypeMustUseSameDependencyPropertyInGetterAndSetter : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WA1212";
        private const string Title = "DependencyProperty CLR property use same dependency proiperty in getter and setter.";
        private const string MessageFormat = "Property '{0}' must access same dependency property in getter and setter";
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

            var setter = propertyDeclaration.GetDependencyPropertyFromSetter();
            var getter = propertyDeclaration.GetDependencyPropertyFromGetter();
            if (getter == null || setter == null || getter == setter)
            {
                return;
            }

            if (getter.DependencyPropertyKey() == setter)
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, propertyDeclaration.GetLocation(), propertyDeclaration.Name()));
        }
    }
}