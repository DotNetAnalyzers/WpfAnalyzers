namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0042AvoidSideEffectsInClrAccessors : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0042";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Avoid side effects in CLR accessors.",
            messageFormat: "Avoid side effects in CLR accessors.",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Avoid side effects in CLR accessors.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(HandleMethod, SyntaxKind.MethodDeclaration);
        }

        private static void HandleMethod(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.Node is MethodDeclarationSyntax methodDeclaration &&
                context.ContainingSymbol is IMethodSymbol method)
            {
                if (ClrMethod.IsPotentialClrGetMethod(method))
                {
                    HandlePotentialGetMethod(context, methodDeclaration.Body);
                }

                if (ClrMethod.IsPotentialClrSetMethod(method))
                {
                    HandlePotentialSetMethod(context, methodDeclaration.Body);
                }
            }
        }

        private static void HandlePotentialGetMethod(SyntaxNodeAnalysisContext context, BlockSyntax body)
        {
            if (body == null)
            {
                return;
            }

            using (var walker = ClrGetterWalker.Borrow(context.SemanticModel, context.CancellationToken, context.Node))
            {
                if (walker.HasError)
                {
                    return;
                }

                if (walker.IsSuccess)
                {
                    var returnStatementSyntax = walker.GetValue.FirstAncestorOrSelf<ReturnStatementSyntax>();
                    foreach (var statement in body.Statements)
                    {
                        if (statement != returnStatementSyntax)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptor, statement.GetLocation()));
                            return;
                        }
                    }
                }
            }
        }

        private static void HandlePotentialSetMethod(SyntaxNodeAnalysisContext context, BlockSyntax body)
        {
            if (body == null)
            {
                return;
            }

            using (var walker = ClrSetterWalker.Borrow(context.SemanticModel, context.CancellationToken, context.Node))
            {
                if (walker.HasError)
                {
                    return;
                }

                if (walker.IsSuccess)
                {
                    foreach (var statement in body.Statements)
                    {
                        if ((statement as ExpressionStatementSyntax)?.Expression != walker.SetValue &&
                            (statement as ExpressionStatementSyntax)?.Expression != walker.SetCurrentValue)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptor, statement.GetLocation()));
                            return;
                        }
                    }
                }
            }
        }
    }
}