namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0002BackingFieldShouldMatchRegisteredName : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0002";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Backing field for a DependencyPropertyKey should match registered name.",
            messageFormat: "Field '{0}' that is backing field for the DependencyPropertyKey registered as '{1}' must be named '{1}PropertyKey'",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "A DependencyPropertyKey's backing field must be named with the name it is registered with suffixed by 'PropertyKey'",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Handle, SyntaxKind.FieldDeclaration);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.ContainingSymbol is IFieldSymbol field &&
                context.Node is FieldDeclarationSyntax fieldDeclaration &&
                field.Type == KnownSymbol.DependencyPropertyKey)
            {
                if (DependencyProperty.TryGetRegisteredName(field, context.SemanticModel, context.CancellationToken, out string registeredName))
                {
                    if (!field.Name.IsParts(registeredName, "PropertyKey"))
                    {
                        var identifier = fieldDeclaration.Declaration.Variables.First().Identifier;
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, identifier.GetLocation(), field.Name, registeredName));
                    }
                }
            }
        }
    }
}