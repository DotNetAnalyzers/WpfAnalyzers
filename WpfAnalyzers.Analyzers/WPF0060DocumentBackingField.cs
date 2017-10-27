namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0060DocumentBackingField : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0060";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Backing field for a DependencyProperty is missing docs.",
            messageFormat: "Backing field for a DependencyProperty is missing docs.",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Hidden,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "Backing field for a DependencyProperty is missing docs.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Handle, SyntaxKind.FieldDeclaration, SyntaxKind.PropertyDeclaration);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if ((context.ContainingSymbol.DeclaredAccessibility == Accessibility.Public ||
                 context.ContainingSymbol.DeclaredAccessibility == Accessibility.Internal) &&
                GetDocumentationCommentTrivia(context.Node) == null &&
                BackingFieldOrProperty.TryCreate(context.ContainingSymbol, out var fieldOrProperty) &&
                DependencyProperty.TryGetRegisteredName(fieldOrProperty, context.SemanticModel, context.CancellationToken, out var registeredName))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptor,
                        context.Node.GetLocation(),
                        fieldOrProperty.Name,
                        registeredName));
            }
        }

        private static DocumentationCommentTriviaSyntax GetDocumentationCommentTrivia(SyntaxNode node)
        {
            if (node == null ||
                !node.HasLeadingTrivia)
            {
                return null;
            }

            foreach (var leadingTrivia in node.GetLeadingTrivia())
            {
                if (leadingTrivia.GetStructure() is DocumentationCommentTriviaSyntax structure)
                {
                    return structure;
                }
            }

            return null;
        }
    }
}