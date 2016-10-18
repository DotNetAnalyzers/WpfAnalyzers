namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;
    using System.Threading;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0007ValidateValueCallbackCallbackShouldMatchRegisteredName : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0007";
        private const string Title = "Name of ValidateValueCallback should match registered name.";
        private const string MessageFormat = "Method '{0}' should be named '{1}'";
        private const string Description = "Name of ValidateValueCallback should match registered name.";
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
            context.RegisterSyntaxNodeAction(HandleInvocation, SyntaxKind.InvocationExpression);
        }

        internal static bool TryGetIdentifierAndRegisteredName(ArgumentSyntax callback, SemanticModel semanticModel, CancellationToken cancellationToken, out IdentifierNameSyntax identifier, out string registeredName)
        {
            registeredName = null;
            string name;
            if (!ValidateValueCallback.TryGetName(callback, semanticModel, cancellationToken, out identifier, out name))
            {
                return false;
            }

            return ValidateValueCallback.TryGetRegisteredName(callback, semanticModel, cancellationToken, out registeredName);
        }

        private static void HandleInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = context.Node as InvocationExpressionSyntax;
            if (invocation == null || invocation.IsMissing)
            {
                return;
            }

            ArgumentSyntax callback;
            if (!ValidateValueCallback.TryGetValidateValueCallback(
                    invocation,
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
                if (!nameExpression.Identifier.ValueText.IsParts(registeredName, "ValidateValue"))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, nameExpression.GetLocation(), nameExpression, $"{registeredName}ValidateValue"));
                }
            }
        }
    }
}
