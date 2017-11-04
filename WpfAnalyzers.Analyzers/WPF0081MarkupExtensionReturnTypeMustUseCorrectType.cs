namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0081MarkupExtensionReturnTypeMustUseCorrectType : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0081";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "MarkupExtensionReturnType must use correct types.",
            messageFormat: "MarkupExtensionReturnType must use correct types.",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "MarkupExtensionReturnType must use correct types.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(HandleDeclaration, SyntaxKind.Attribute);
        }

        private static void HandleDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.ContainingSymbol is ITypeSymbol type &&
                context.Node is AttributeSyntax attribute &&
                !type.IsAbstract &&
                type.Is(KnownSymbol.MarkupExtension) &&
                Attribute.IsType(attribute, KnownSymbol.MarkupExtensionReturnTypeAttribute, context.SemanticModel, context.CancellationToken) &&
                attribute.FirstAncestor<ClassDeclarationSyntax>() is ClassDeclarationSyntax classDeclaration &&
                MarkupExtension.TryGetReturnType(classDeclaration, context.SemanticModel, context.CancellationToken, out var returnType))
            {
                if (Attribute.TryGetArgument(attribute, 0, "returnType", out var arg) &&
                    TryGetType(arg, context.SemanticModel, context.CancellationToken, out var argType) &&
                    !ReferenceEquals(argType, returnType))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, arg.GetLocation()));
                }
            }
        }

        private static bool TryGetType(AttributeArgumentSyntax arg, SemanticModel semanticModel, CancellationToken cancellationToken, out ITypeSymbol type)
        {
            type = null;
            if (arg.Expression is TypeOfExpressionSyntax typeOf)
            {
                type = semanticModel.GetTypeInfoSafe(typeOf.Type, cancellationToken).Type;
            }

            return type != null;
        }
    }
}