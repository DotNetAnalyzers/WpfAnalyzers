namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;

    using WpfAnalyzers.DependencyProperties.Internals;

    /// <summary>
    /// DependencyProperty field must be named &lt;Name&gt;Property
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WA1201DependencyPropertyFieldMustBeStaticReadOnly : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WA1201";
        private const string Title = "DependencyProperty field must be static and readonly";
        private const string MessageFormat = "DependencyProperty '{0}' field must be static and readonly";
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
            context.RegisterSyntaxNodeAction(HandleFieldDeclaration, SyntaxKind.FieldDeclaration);
        }

        private static void HandleFieldDeclaration(SyntaxNodeAnalysisContext context)
        {
            var fieldSymbol = (IFieldSymbol)context.ContainingSymbol;
            if (!fieldSymbol.IsDependencyPropertyField())
            {
                return;
            }

            if (!fieldSymbol.IsReadOnly)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, context.Node.GetLocation(), fieldSymbol.Name));
            }

            if (!fieldSymbol.IsStatic)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, context.Node.GetLocation(), fieldSymbol.Name));
            }
        }
    }
}