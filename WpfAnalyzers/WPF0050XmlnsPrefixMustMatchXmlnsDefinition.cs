namespace WpfAnalyzers;

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

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Descriptors.WPF0050XmlnsPrefixMustMatchXmlnsDefinition);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(x => Handle(x), SyntaxKind.Attribute);
    }

    private static void Handle(SyntaxNodeAnalysisContext context)
    {
        if (!context.IsExcludedFromAnalysis() &&
            context.Node is AttributeSyntax attribute)
        {
            QualifiedType correspondingType;
            AttributeSyntax xmlnsAttribute;
            if (context.SemanticModel.TryGetNamedType(attribute, KnownSymbols.XmlnsPrefixAttribute, context.CancellationToken, out _))
            {
                xmlnsAttribute = attribute;
                correspondingType = KnownSymbols.XmlnsDefinitionAttribute;
            }
            else if (context.SemanticModel.TryGetNamedType(attribute, KnownSymbols.XmlnsDefinitionAttribute, context.CancellationToken, out _))
            {
                xmlnsAttribute = attribute;
                correspondingType = KnownSymbols.XmlnsPrefixAttribute;
            }
            else
            {
                return;
            }

            if (!xmlnsAttribute.TryFindArgument(0, KnownSymbols.XmlnsDefinitionAttribute.XmlNamespaceArgumentName, out var arg))
            {
                return;
            }

            if (!context.SemanticModel.TryGetConstantValue(arg.Expression, context.CancellationToken, out string? xmlNamespace))
            {
                return;
            }

            var compilation = xmlnsAttribute.FirstAncestorOrSelf<CompilationUnitSyntax>();
            if (compilation is null)
            {
                return;
            }

            foreach (var correspondingAttribute in AttributeExt.FindAttributes(compilation, correspondingType, context.SemanticModel, context.CancellationToken))
            {
                if (correspondingAttribute.TryFindArgument(0, KnownSymbols.XmlnsDefinitionAttribute.XmlNamespaceArgumentName, out var correspondingArg))
                {
                    if (!context.SemanticModel.TryGetConstantValue(correspondingArg.Expression, context.CancellationToken, out string? mappedNameSpace))
                    {
                        return;
                    }

                    if (mappedNameSpace == xmlNamespace)
                    {
                        return;
                    }
                }
            }

            var attributeName = correspondingType == KnownSymbols.XmlnsPrefixAttribute
                ? XmlnsPrefix
                : XmlnsDefinition;
            context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0050XmlnsPrefixMustMatchXmlnsDefinition, arg.GetLocation(), attributeName, xmlNamespace));
        }
    }
}