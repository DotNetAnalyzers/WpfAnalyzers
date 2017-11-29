namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0034AttachedPropertyBrowsableForTypeAttributeArgument : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0034";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Use correct argument for [AttachedPropertyBrowsableForType]",
            messageFormat: "Use [AttachedPropertyBrowsableForType(typeof({0})]",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Use correct argument for [AttachedPropertyBrowsableForType]",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(HandleDeclaration, SyntaxKind.AttributeArgument);
        }

        private static void HandleDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.Node is AttributeArgumentSyntax attributeArgument &&
                attributeArgument.FirstAncestor<AttributeSyntax>() is AttributeSyntax attribute &&
                attribute.FirstAncestor<MethodDeclarationSyntax>() is MethodDeclarationSyntax methodDeclaration &&
                AttachedPropertyBrowsableForType.TryGetAttribute(methodDeclaration, context.SemanticModel, context.CancellationToken, out _) &&
                AttachedPropertyBrowsableForType.TryGetParameterType(methodDeclaration, context.SemanticModel, context.CancellationToken, out var parameterType) &&
                TryGetType(attributeArgument, context.SemanticModel, context.CancellationToken, out var argumentType) &&
                !ReferenceEquals(parameterType, argumentType))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, attributeArgument.GetLocation(), parameterType.ToMinimalDisplayString(context.SemanticModel, attributeArgument.SpanStart)));
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