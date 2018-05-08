namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Linq;
    using Gu.Roslyn.AnalyzerExtensions;
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
            category: AnalyzerCategory.XmlnsDefinition,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
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

            if (context.Node is AttributeSyntax attribute &&
                Attribute.IsType(attribute, KnownSymbol.XmlnsDefinitionAttribute, context.SemanticModel, context.CancellationToken) &&
                Attribute.TryFindArgument(attribute, 1, KnownSymbol.XmlnsDefinitionAttribute.ClrNamespaceArgumentName, out var arg))
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
