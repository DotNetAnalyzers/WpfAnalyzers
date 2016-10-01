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

            var symbolInfo = context.SemanticModel.GetSymbolInfo(declaration, context.CancellationToken);
            if (symbolInfo.Symbol.ContainingType.Name != Names.DependencyObject)
            {
                return;
            }

            var argument = declaration.ArgumentList.Arguments[0];
            var dp = context.SemanticModel.GetSymbolInfo(argument.Expression);
            if (dp.Symbol.DeclaringSyntaxReferences.Length != 1)
            {
                return;
            }

            var declarator = dp.Symbol
                               .DeclaringSyntaxReferences[0]
                               .GetSyntax(context.CancellationToken) as VariableDeclaratorSyntax;
            if (declarator == null)
            {
                return;
            }

            var member = declarator.Initializer.Value as MemberAccessExpressionSyntax;
            if (member.IsDependencyPropertyKeyProperty())
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, argument.GetLocation(), argument, member?.Expression));
            }
        }
    }
}