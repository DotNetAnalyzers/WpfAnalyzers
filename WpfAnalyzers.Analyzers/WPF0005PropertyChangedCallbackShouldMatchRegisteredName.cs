namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0005PropertyChangedCallbackShouldMatchRegisteredName : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0005";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Name of PropertyChangedCallback should match registered name.",
            messageFormat: "Method '{0}' should be named '{1}'",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "Name of PropertyChangedCallback should match registered name.",
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
            return PropertyChangedCallback.TryGetName(callback, semanticModel, cancellationToken, out identifier, out string _) &&
                   PropertyChangedCallback.TryGetRegisteredName(callback, semanticModel, cancellationToken, out registeredName);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.Node is ObjectCreationExpressionSyntax objectCreation &&
                context.ContainingSymbol.IsStatic)
            {
                if (!PropertyMetaData.TryGetPropertyChangedCallback(
                    objectCreation,
                    context.SemanticModel,
                    context.CancellationToken,
                    out var callback))
                {
                    return;
                }

                if (TryGetIdentifierAndRegisteredName(
                    callback,
                    context.SemanticModel,
                    context.CancellationToken,
                    out var nameExpression,
                    out var registeredName))
                {
                    if (!nameExpression.Identifier.ValueText.IsParts("On", registeredName, "Changed"))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, nameExpression.GetLocation(), nameExpression, $"On{registeredName}Changed"));
                    }
                }
            }
        }
    }
}
