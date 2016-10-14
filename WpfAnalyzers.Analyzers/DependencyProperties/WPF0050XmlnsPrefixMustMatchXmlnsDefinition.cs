namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0050XmlnsPrefixMustMatchXmlnsDefinition : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0050";
        private const string Title = "XmlnsPrefix must map to the same url as XmlnsDefinition.";
        private const string MessageFormat = "There is no [XmlnsDefinition] mapping to '{0}'";
        private const string Description = "[XmlnsPrefix] must have a corresponding [XmlnsDefinition] mapping to the same url.";
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
            context.RegisterSyntaxNodeAction(HandleDeclaration, SyntaxKind.Attribute);
        }

        private static void HandleDeclaration(SyntaxNodeAnalysisContext context)
        {
            var attributeSyntax = context.Node as AttributeSyntax;
            if (attributeSyntax == null ||
                attributeSyntax.IsMissing)
            {
                return;
            }

            if (!Attribute.TryGetAttribute(
                attributeSyntax,
                "System.Windows.Markup.XmlnsPrefixAttribute",
                context.SemanticModel,
                context.CancellationToken,
                out attributeSyntax))
            {
                return;
            }

            string xmlNamespace;
            if (!Attribute.TryGetArgumentStringValue(attributeSyntax, 0, context.SemanticModel, context.CancellationToken, out xmlNamespace))
            {
                return;
            }

            var compilation = attributeSyntax.FirstAncestorOrSelf<CompilationUnitSyntax>();
            if (compilation == null)
            {
                return;
            }

            foreach (var attributeList in compilation.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    AttributeSyntax xmlnsDefAttribute;
                    if (Attribute.TryGetAttribute(attribute, "System.Windows.Markup.XmlnsDefinitionAttribute", context.SemanticModel, context.CancellationToken, out xmlnsDefAttribute))
                    {
                        string mappedNameSpace;
                        if (Attribute.TryGetArgumentStringValue(xmlnsDefAttribute, 0, context.SemanticModel, context.CancellationToken, out mappedNameSpace))
                        {
                            if (mappedNameSpace == xmlNamespace)
                            {
                                return;
                            }
                        }
                    }
                }
            }

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, attributeSyntax.GetLocation(), xmlNamespace));
        }
    }
}
