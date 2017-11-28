namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0080MarkupExtensionDoesNotHaveAttribute : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0080";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Add MarkupExtensionReturnType attribute.",
            messageFormat: "Add MarkupExtensionReturnType attribute.",
            category: AnalyzerCategory.MarkupExtension,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Add MarkupExtensionReturnType attribute.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(HandleDeclaration, SyntaxKind.ClassDeclaration);
        }

        private static void HandleDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.ContainingSymbol is ITypeSymbol type &&
                context.Node is ClassDeclarationSyntax classDeclaration &&
                type.Is(KnownSymbol.MarkupExtension) &&
                !Attribute.TryGetAttribute(classDeclaration, KnownSymbol.MarkupExtensionReturnTypeAttribute, context.SemanticModel, context.CancellationToken, out _))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, classDeclaration.Identifier.GetLocation()));
            }
        }
    }
}