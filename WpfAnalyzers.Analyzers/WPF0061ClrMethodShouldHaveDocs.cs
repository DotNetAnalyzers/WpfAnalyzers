namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0061ClrMethodShouldHaveDocs : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0061";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "CLR accessor for attached property should have documentation.",
            messageFormat: "CLR accessor for attached property should have documentation.",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "CLR accessor for attached property should have documentation.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(HandleDeclaration, SyntaxKind.MethodDeclaration);
        }

        private static void HandleDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.Node is MethodDeclarationSyntax methodDeclaration &&
                context.ContainingSymbol is IMethodSymbol method &&
                !methodDeclaration.HasDocumentation() &&
                (method.DeclaredAccessibility == Accessibility.Public ||
                 method.DeclaredAccessibility == Accessibility.Internal))
            {
                if (ClrMethod.IsAttachedGetMethod(method, context.SemanticModel, context.CancellationToken, out _))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, methodDeclaration.GetLocation()));
                }
                else if (ClrMethod.IsAttachedSetMethod(method, context.SemanticModel, context.CancellationToken, out _))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, methodDeclaration.GetLocation()));
                }
            }
        }
    }
}