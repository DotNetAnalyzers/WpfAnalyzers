namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0040SetUsingDependencyPropertyKey : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0040";

        private const string Title = "A readonly DependencyProperty must be set with DependencyPropertyKey.";

        private const string MessageFormat = "Set '{0}' using '{1}'";

        private const string Description = Title;

        private static readonly string HelpLink = WpfAnalyzers.HelpLink.ForId(DiagnosticId);

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
                                                                      DiagnosticId,
                                                                      Title,
                                                                      MessageFormat,
                                                                      AnalyzerCategory.DependencyProperties,
                                                                      DiagnosticSeverity.Error,
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

        private static void HandleInvocation(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            var invocation = context.Node as InvocationExpressionSyntax;
            if (!DependencyObject.TryGetSetValueArguments(invocation, context.SemanticModel, context.CancellationToken, out ArgumentSyntax property, out IFieldSymbol setField, out ArgumentSyntax value) &&
!DependencyObject.TryGetSetCurrentValueArguments(invocation, context.SemanticModel, context.CancellationToken, out property, out setField, out value))
            {
                return;
            }

            if (setField == null ||
                setField.Type == KnownSymbol.DependencyPropertyKey)
            {
                return;
            }

            if (DependencyProperty.TryGetDependencyPropertyKeyField(
    setField,
    context.SemanticModel,
    context.CancellationToken,
    out IFieldSymbol keyField))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, property.GetLocation(), property, DependencyProperty.CreateArgument(keyField, context.SemanticModel, property.SpanStart)));
            }
        }
    }
}