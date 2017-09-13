namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0001BackingFieldShouldMatchRegisteredName : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0001";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Backing field for a DependencyProperty should match registered name.",
            messageFormat: "Field '{0}' that is backing field for the DependencyProperty registered as '{1}' must be named '{1}Property'",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "A dependency property's backing field must be named with the name it is registered with suffixed by 'Property'",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(HandleFieldDeclaration, SyntaxKind.FieldDeclaration);
        }

        private static void HandleFieldDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            var fieldDeclaration = context.Node as FieldDeclarationSyntax;
            if (fieldDeclaration == null ||
                fieldDeclaration.IsMissing)
            {
                return;
            }

            var field = context.ContainingSymbol as IFieldSymbol;
            if (field == null ||
                field.Type != KnownSymbol.DependencyProperty)
            {
                return;
            }

            if (DependencyProperty.TryGetRegisteredName(field, context.SemanticModel, context.CancellationToken, out string registeredName))
            {
                if (!field.Name.IsParts(registeredName, "Property"))
                {
                    var identifier = fieldDeclaration.Declaration.Variables.First().Identifier;
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, identifier.GetLocation(), field.Name, registeredName));
                }
            }
        }
    }
}
