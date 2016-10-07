namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0030BackingFieldShouldBeStaticReadonly : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0030";
        private const string Title = "Backing field for a DependencyProperty should be static and readonly.";
        private const string MessageFormat = "Field '{0}' is backing field for a DependencyProperty and should be static and readonly.";
        private const string Description = "Backing field for a DependencyProperty should be static and readonly.";
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
            context.RegisterSyntaxNodeAction(HandleFieldDeclaration, SyntaxKind.FieldDeclaration);
        }

        private static void HandleFieldDeclaration(SyntaxNodeAnalysisContext context)
        {
            var fieldDeclaration = context.Node as FieldDeclarationSyntax;
            if (fieldDeclaration == null || fieldDeclaration.IsMissing)
            {
                return;
            }

            if (!fieldDeclaration.IsDependencyPropertyField() &&
                !fieldDeclaration.IsDependencyPropertyKeyField())
            {
                return;
            }

            var fieldSymbol = (IFieldSymbol)context.ContainingSymbol;
            if (!fieldSymbol.IsReadOnly || !fieldSymbol.IsStatic)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, context.Node.GetLocation(), fieldSymbol.Name, fieldSymbol.Type.Name));
            }
        }
    }
}