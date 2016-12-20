namespace WpfAnalyzers.DependencyProperties
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
        private const string Title = "Name of PropertyChangedCallback should match registered name.";
        private const string MessageFormat = "Method '{0}' should be named '{1}'";
        private const string Description = "Name of PropertyChangedCallback should match registered name.";
        private static readonly string HelpLink = WpfAnalyzers.HelpLink.ForId(DiagnosticId);

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            AnalyzerCategory.DependencyProperties,
            DiagnosticSeverity.Warning,
            AnalyzerConstants.EnabledByDefault,
            Description,
            HelpLink);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(HandleObjectCreation, SyntaxKind.ObjectCreationExpression);
        }

        internal static bool TryGetIdentifierAndRegisteredName(ArgumentSyntax callback, SemanticModel semanticModel, CancellationToken cancellationToken, out IdentifierNameSyntax identifier, out string registeredName)
        {
            registeredName = null;
            string name;
            if (!PropertyChangedCallback.TryGetName(callback, semanticModel, cancellationToken, out identifier, out name))
            {
                return false;
            }

            return PropertyChangedCallback.TryGetRegisteredName(callback, semanticModel, cancellationToken, out registeredName);
        }

        private static void HandleObjectCreation(SyntaxNodeAnalysisContext context)
        {
            var objectCreation = context.Node as ObjectCreationExpressionSyntax;
            if (context.SemanticModel == null ||
                objectCreation == null ||
                objectCreation.IsMissing)
            {
                return;
            }

            ArgumentSyntax callback;
            if (!PropertyMetaData.TryGetPropertyChangedCallback(
                    objectCreation,
                    context.SemanticModel,
                    context.CancellationToken,
                    out callback))
            {
                return;
            }

            IdentifierNameSyntax nameExpression;
            string registeredName;
            if (TryGetIdentifierAndRegisteredName(
                callback,
                context.SemanticModel,
                context.CancellationToken,
                out nameExpression,
                out registeredName))
            {
                if (!nameExpression.Identifier.ValueText.IsParts("On", registeredName, "Changed"))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, nameExpression.GetLocation(), nameExpression, $"On{registeredName}Changed"));
                }
            }
        }
    }
}
