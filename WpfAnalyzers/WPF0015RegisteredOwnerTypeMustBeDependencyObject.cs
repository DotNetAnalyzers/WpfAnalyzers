namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0015RegisteredOwnerTypeMustBeDependencyObject : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Descriptors.WPF0015RegisteredOwnerTypeMustBeDependencyObject);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(x => Handle(x), SyntaxKind.InvocationExpression);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.Node is InvocationExpressionSyntax invocation)
            {
                if ((DependencyProperty.TryGetOverrideMetadataCall(invocation, context.SemanticModel, context.CancellationToken, out _) ||
                     DependencyProperty.TryGetAddOwnerCall(invocation, context.SemanticModel, context.CancellationToken, out _)) &&
                    invocation.TryGetArgumentAtIndex(0, out var argument))
                {
                    HandleArgument(context, argument);
                }

                if ((RegisterInvocation.TryMatchRegister(invocation, context.SemanticModel, context.CancellationToken, out _) ||
                     RegisterInvocation.TryMatchRegisterReadOnly(invocation, context.SemanticModel, context.CancellationToken, out _)) &&
                    invocation.TryGetArgumentAtIndex(2, out argument))
                {
                    HandleArgument(context, argument);
                }
            }
        }

        private static void HandleArgument(SyntaxNodeAnalysisContext context, ArgumentSyntax argument)
        {
            if (argument.TryGetTypeofValue(context.SemanticModel, context.CancellationToken, out var ownerType) &&
                !ownerType.IsAssignableTo(KnownSymbols.DependencyObject, context.Compilation))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0015RegisteredOwnerTypeMustBeDependencyObject, argument.GetLocation(), KnownSymbols.DependencyProperty.RegisterAttached.Name));
            }
        }
    }
}
