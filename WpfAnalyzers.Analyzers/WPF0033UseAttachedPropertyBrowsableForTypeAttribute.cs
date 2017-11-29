namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0033UseAttachedPropertyBrowsableForTypeAttribute : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0033";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Add [AttachedPropertyBrowsableForType]",
            messageFormat: "Add [AttachedPropertyBrowsableForType(typeof({0})]",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Add [AttachedPropertyBrowsableForType]",
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
                methodDeclaration.ParameterList != null &&
                methodDeclaration.ParameterList.Parameters.Count == 1 &&
                context.ContainingSymbol is IMethodSymbol method &&
                (method.DeclaredAccessibility == Accessibility.Public ||
                 method.DeclaredAccessibility == Accessibility.Internal) &&
                ClrMethod.IsAttachedGetMethod(method, context.SemanticModel, context.CancellationToken, out _) &&
                !AttachedPropertyBrowsableForType.TryGetAttribute(methodDeclaration, context.SemanticModel, context.CancellationToken, out _))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, methodDeclaration.Identifier.GetLocation(), method.Parameters[0].Type.ToMinimalDisplayString(context.SemanticModel, methodDeclaration.SpanStart)));
            }
        }
    }
}