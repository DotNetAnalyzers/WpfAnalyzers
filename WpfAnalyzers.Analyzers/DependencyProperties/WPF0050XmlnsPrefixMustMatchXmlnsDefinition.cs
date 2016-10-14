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
        private const string XmlnsPrefix = "XmlnsPrefix";
        private const string XmlnsPrefixAttribute = "System.Windows.Markup.XmlnsPrefixAttribute";
        private const string XmlnsDefinition = "XmlnsDefinition";
        private const string XmlnsDefinitionAttribute = "System.Windows.Markup.XmlnsDefinitionAttribute";
        private const string Title = "XmlnsPrefix must map to the same url as XmlnsDefinition.";
        private const string MessageFormat = "There is no [{0}] mapping to '{1}'";
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

            string correspondingType = null;
            AttributeSyntax attribute;
            if (Attribute.TryGetAttribute(attributeSyntax, XmlnsPrefixAttribute, context.SemanticModel, context.CancellationToken, out attribute))
            {
                correspondingType = XmlnsDefinitionAttribute;
            }

            if (attribute == null && Attribute.TryGetAttribute(attributeSyntax, XmlnsDefinitionAttribute, context.SemanticModel, context.CancellationToken, out attribute))
            {
                correspondingType = XmlnsPrefixAttribute;
            }

            if (correspondingType == null || attribute == null)
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

            foreach (var corresponding in Attribute.FindAttributes(compilation, correspondingType, context.SemanticModel, context.CancellationToken))
            {
                string mappedNameSpace;
                if (Attribute.TryGetArgumentStringValue(corresponding, 0, context.SemanticModel, context.CancellationToken, out mappedNameSpace))
                {
                    if (mappedNameSpace == xmlNamespace)
                    {
                        return;
                    }
                }
            }

            var attributeName = correspondingType == XmlnsPrefixAttribute
                                    ? XmlnsPrefix
                                    : XmlnsDefinition;
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, attributeSyntax.GetLocation(), attributeName, xmlNamespace));
        }
    }
}
