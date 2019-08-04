namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0050XmlnsPrefixMustMatchXmlnsDefinition : DiagnosticAnalyzer
    {
        private const string XmlnsPrefix = "XmlnsPrefix";
        private const string XmlnsDefinition = "XmlnsDefinition";

        internal static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: "WPF0050",
            title: "XmlnsPrefix must map to the same url as XmlnsDefinition.",
            messageFormat: "There is no [{0}] mapping to '{1}'",
            category: AnalyzerCategory.XmlnsDefinition,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "[XmlnsPrefix] must have a corresponding [XmlnsDefinition] mapping to the same url.");

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(x => Handle(x), SyntaxKind.Attribute);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.Node is AttributeSyntax attribute)
            {
                QualifiedType correspondingType;
                AttributeSyntax xmlnsAttribute;
                if (Attribute.IsType(attribute, KnownSymbol.XmlnsPrefixAttribute, context.SemanticModel, context.CancellationToken))
                {
                    xmlnsAttribute = attribute;
                    correspondingType = KnownSymbol.XmlnsDefinitionAttribute;
                }
                else if (Attribute.IsType(attribute, KnownSymbol.XmlnsDefinitionAttribute, context.SemanticModel, context.CancellationToken))
                {
                    xmlnsAttribute = attribute;
                    correspondingType = KnownSymbol.XmlnsPrefixAttribute;
                }
                else
                {
                    return;
                }

                if (!Attribute.TryFindArgument(xmlnsAttribute, 0, KnownSymbol.XmlnsDefinitionAttribute.XmlNamespaceArgumentName, out var arg))
                {
                    return;
                }

                if (!context.SemanticModel.TryGetConstantValue(arg.Expression, context.CancellationToken, out string xmlNamespace))
                {
                    return;
                }

                var compilation = xmlnsAttribute.FirstAncestorOrSelf<CompilationUnitSyntax>();
                if (compilation == null)
                {
                    return;
                }

                foreach (var correspondingAttribute in AttributeExt.FindAttributes(compilation, correspondingType, context.SemanticModel, context.CancellationToken))
                {
                    if (Attribute.TryFindArgument(correspondingAttribute, 0, KnownSymbol.XmlnsDefinitionAttribute.XmlNamespaceArgumentName, out var correspondingArg))
                    {
                        if (!context.SemanticModel.TryGetConstantValue(correspondingArg.Expression, context.CancellationToken, out string mappedNameSpace))
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
}
