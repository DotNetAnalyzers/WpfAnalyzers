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
            var invocation = context.Node as InvocationExpressionSyntax;
            ArgumentSyntax property;
            ArgumentSyntax value;
            if (!DependencyObject.TryGetSetValueArguments(invocation, context.SemanticModel, context.CancellationToken, out property, out value) &&
                !DependencyObject.TryGetSetCurrentValueArguments(invocation, context.SemanticModel, context.CancellationToken, out property, out value))
            {
                return;
            }

            FieldDeclarationSyntax field;
            if (property.TryGetDependencyPropertyFieldDeclaration(context.SemanticModel, context.CancellationToken, out field))
            {
                FieldDeclarationSyntax key;
                if (field.TryGetDependencyPropertyKey(out key))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, property.GetLocation(), property, key.Name()));
                }
            }
        }
    }
}