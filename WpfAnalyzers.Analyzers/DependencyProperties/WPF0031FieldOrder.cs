namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0031FieldOrder : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0031";
        private const string Title = "DependencyPropertyKey field must come before DependencyProperty field.";
        private const string MessageFormat = "Field '{0}' must come before '{1}'";
        private const string Description = Title;
        private static readonly string HelpLink = WpfAnalyzers.HelpLink.ForId(DiagnosticId);

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
            context.RegisterSyntaxNodeAction(HandleDeclaration, SyntaxKind.FieldDeclaration);
        }

        private static void HandleDeclaration(SyntaxNodeAnalysisContext context)
        {
            var declaration = context.Node as FieldDeclarationSyntax;
            if (declaration == null || declaration.IsMissing || !declaration.IsDependencyPropertyField())
            {
                return;
            }

            FieldDeclarationSyntax key;
            if (!declaration.TryGetDependencyPropertyKey(out key))
            {
                return;
            }

            if (key.SpanStart < declaration.SpanStart)
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, declaration.GetLocation(), key.Name(), declaration.Name()));
        }
    }
}