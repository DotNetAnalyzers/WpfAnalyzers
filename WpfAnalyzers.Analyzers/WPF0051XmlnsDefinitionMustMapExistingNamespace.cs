namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0051XmlnsDefinitionMustMapExistingNamespace : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0051";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "XmlnsDefinition must map to existing namespace.",
            messageFormat: "[XmlnsDefinition] maps to '{0}' that does not exist.",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "XmlnsDefinition must map to existing namespace.",
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

            var attributeSyntax = context.Node as AttributeSyntax;
            if (attributeSyntax == null ||
                attributeSyntax.IsMissing)
            {
                return;
            }

            var xmlnsDefinitionAttributeType = KnownSymbol.XmlnsDefinitionAttribute;
            if (Attribute.TryGetAttribute(attributeSyntax, xmlnsDefinitionAttributeType, context.SemanticModel, context.CancellationToken, out AttributeSyntax _) &&
                Attribute.TryGetArgument(attributeSyntax, 1, xmlnsDefinitionAttributeType.ClrNamespaceArgumentName, out AttributeArgumentSyntax arg))
            {
                if (context.SemanticModel.TryGetConstantValue(arg.Expression, context.CancellationToken, out string @namespace))
                {
                    if (context.Compilation.GetSymbolsWithName(x => !string.IsNullOrEmpty(x) && @namespace.EndsWith(x), SymbolFilter.Namespace)
            .All(x => x.ToMinimalDisplayString(context.SemanticModel, 0) != @namespace))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, arg.GetLocation(), arg));
                    }
                }
            }
        }
    }
}
