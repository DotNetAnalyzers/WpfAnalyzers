namespace WpfAnalyzers
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

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            DiagnosticId,
            "Name of ValidateValueCallback should match registered name.",
            "Method '{0}' should be named '{1}'",
            AnalyzerCategory.DependencyProperties,
            DiagnosticSeverity.Warning,
            AnalyzerConstants.EnabledByDefault,
            "Name of ValidateValueCallback should match registered name.",
            HelpLink.ForId(DiagnosticId));

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
            if (!ValidateValueCallback.TryGetName(callback, semanticModel, cancellationToken, out identifier, out string _))
            {
                return false;
            }

            return ValidateValueCallback.TryGetRegisteredName(callback, semanticModel, cancellationToken, out registeredName);
        }

        private static void HandleInvocation(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            var invocation = context.Node as InvocationExpressionSyntax;
            if (invocation == null || invocation.IsMissing)
            {
                return;
            }

            if (!ValidateValueCallback.TryGetValidateValueCallback(
        invocation,
        context.SemanticModel,
        context.CancellationToken,
        out ArgumentSyntax callback))
            {
                return;
            }

            if (TryGetIdentifierAndRegisteredName(
callback,
context.SemanticModel,
context.CancellationToken,
out IdentifierNameSyntax nameExpression,
out string registeredName))
            {
                if (!nameExpression.Identifier.ValueText.IsParts(registeredName, "ValidateValue"))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, nameExpression.GetLocation(), nameExpression, $"{registeredName}ValidateValue"));
                }
            }
        }
    }
}
