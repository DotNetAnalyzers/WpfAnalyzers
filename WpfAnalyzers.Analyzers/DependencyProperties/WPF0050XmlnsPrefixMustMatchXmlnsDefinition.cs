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
        private const string XmlnsDefinition = "XmlnsDefinition";
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

            QualifiedType correspondingType = null;
            AttributeSyntax xmlnsAttribute;
            if (Attribute.TryGetAttribute(attributeSyntax, KnownSymbol.XmlnsPrefixAttribute, context.SemanticModel, context.CancellationToken, out xmlnsAttribute))
            {
                correspondingType = KnownSymbol.XmlnsDefinitionAttribute;
            }

            if (xmlnsAttribute == null && Attribute.TryGetAttribute(attributeSyntax, KnownSymbol.XmlnsDefinitionAttribute, context.SemanticModel, context.CancellationToken, out xmlnsAttribute))
            {
                correspondingType = KnownSymbol.XmlnsPrefixAttribute;
            }

            if (correspondingType == null || xmlnsAttribute == null)
            {
                return;
            }

            AttributeArgumentSyntax arg;
            if (!Attribute.TryGetArgument(xmlnsAttribute, 0, KnownSymbol.XmlnsDefinitionAttribute.XmlNamespaceArgumentName, out arg))
            {
                return;
            }

            string xmlNamespace;
            if (!context.SemanticModel.TryGetConstantValue(arg.Expression, context.CancellationToken, out xmlNamespace))
            {
                return;
            }

            var compilation = xmlnsAttribute.FirstAncestorOrSelf<CompilationUnitSyntax>();
            if (compilation == null)
            {
                return;
            }

            foreach (var correspondingAttribute in Attribute.FindAttributes(compilation, correspondingType, context.SemanticModel, context.CancellationToken))
            {
                AttributeArgumentSyntax correspondingArg;
                if (Attribute.TryGetArgument(correspondingAttribute, 0, KnownSymbol.XmlnsDefinitionAttribute.XmlNamespaceArgumentName, out correspondingArg))
                {
                    string mappedNameSpace;
                    if (!context.SemanticModel.TryGetConstantValue(correspondingArg.Expression, context.CancellationToken, out mappedNameSpace))
                    {
                        return;
                    }

                    if (mappedNameSpace == xmlNamespace)
                    {
                        return;
                    }
                }
            }

            var attributeName = ReferenceEquals(correspondingType, KnownSymbol.XmlnsPrefixAttribute)
                                    ? XmlnsPrefix
                                    : XmlnsDefinition;
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, arg.GetLocation(), attributeName, xmlNamespace));
        }
    }
}
