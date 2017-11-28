namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0006CoerceValueCallbackShouldMatchRegisteredName : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0006";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Name of CoerceValueCallback should match registered name.",
            messageFormat: "Method '{0}' should be named '{1}'",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Name of CoerceValueCallback should match registered name.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Handle, SyntaxKind.ObjectCreationExpression);
        }

        internal static bool TryGetIdentifierAndRegisteredName(ArgumentSyntax callback, SemanticModel semanticModel, CancellationToken cancellationToken, out IdentifierNameSyntax identifier, out string registeredName)
        {
            registeredName = null;
            return CoerceValueCallback.TryGetName(callback, semanticModel, cancellationToken, out identifier, out string _) &&
                   CoerceValueCallback.TryGetRegisteredName(callback, semanticModel, cancellationToken, out registeredName);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.Node is ObjectCreationExpressionSyntax objectCreation &&
                context.ContainingSymbol.IsStatic &&
                PropertyMetadata.TryGetCoerceValueCallback(objectCreation, context.SemanticModel, context.CancellationToken, out var callback) &&
                TryGetIdentifierAndRegisteredName(callback, context.SemanticModel, context.CancellationToken, out var identifier, out var registeredName) &&
                !identifier.Identifier.ValueText.IsParts("Coerce", registeredName))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptor,
                        identifier.GetLocation(),
                        identifier,
                        $"Coerce{registeredName}"));
            }
        }
    }
}
