namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WA1220UseDependencyPropertyKeyForSettingReadOnlyProperties : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WA1220";
        private const string Title = "A readonly DependencyProperty must be set with DependencyPropertyKey.";
        private const string MessageFormat = "Set '{0}' using '{1}'";
        private const string Description = Title;
        private const string HelpLink = "http://stackoverflow.com/";

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
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(HandleInvocation, SyntaxKind.InvocationExpression);
        }

        private static void HandleInvocation(SyntaxNodeAnalysisContext context)
        {
            var declaration = context.Node as InvocationExpressionSyntax;
            if (declaration == null ||
                declaration.IsMissing ||
                declaration.ArgumentList?.Arguments.Count != 2 ||
                !(declaration.IsSetValue() || declaration.IsSetSetCurrentValue()))
            {
                return;
            }

            var symbol = context.ContainingSymbol;
            var argument = declaration.ArgumentList.Arguments[0];
            //declaration.
            //var dependencyProperty = property.Class()
            //    .Field(invocation.ArgumentList.Arguments.First().Expression as IdentifierNameSyntax);

            //context.ReportDiagnostic(Diagnostic.Create(Descriptor, declaration.GetLocation(), key.Name(), declaration.Name()));
        }
    }
}