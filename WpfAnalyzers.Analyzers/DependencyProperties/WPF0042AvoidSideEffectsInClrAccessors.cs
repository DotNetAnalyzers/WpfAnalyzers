namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using WpfAnalyzers;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0042AvoidSideEffectsInClrAccessors : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0042";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Avoid side effects in CLR accessors.",
            messageFormat: "Avoid side effects in CLR accessors.",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
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
            context.RegisterSyntaxNodeAction(HandleGetter, SyntaxKind.GetAccessorDeclaration);
            context.RegisterSyntaxNodeAction(HandleSetter, SyntaxKind.SetAccessorDeclaration);
        }

        private static void HandleMethod(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            var method = (IMethodSymbol)context.ContainingSymbol;
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;
            if (methodDeclaration.Body == null)
            {
                return;
            }

            if (ClrMethod.IsPotentialClrGetMethod(method))
            {
                HandlePotentialGetMethod(context, methodDeclaration.Body);
            }

            if (ClrMethod.IsPotentialClrSetMethod(method))
            {
                HandlePotentialSetMethod(context, methodDeclaration.Body);
            }
        }

        private static void HandleGetter(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            var getter = (AccessorDeclarationSyntax)context.Node;
            var property = (IPropertySymbol)((IMethodSymbol)context.ContainingSymbol).AssociatedSymbol;
            if (getter.Body == null ||
                getter.Body.Statements.Count == 1 ||
                !property.IsPotentialClrProperty())
            {
                return;
            }

            HandlePotentialGetMethod(context, getter.Body);
        }

        private static void HandleSetter(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            var setter = (AccessorDeclarationSyntax)context.Node;
            var property = (IPropertySymbol)((IMethodSymbol)context.ContainingSymbol).AssociatedSymbol;
            if (setter.Body == null ||
                setter.Body.Statements.Count == 1 ||
                !property.IsPotentialClrProperty())
            {
                return;
            }

            HandlePotentialSetMethod(context, setter.Body);
        }

        private static void HandlePotentialGetMethod(SyntaxNodeAnalysisContext context, BlockSyntax body)
        {
            if (body == null)
            {
                return;
            }

            using (var pooled = ClrGetterWalker.Create(context.SemanticModel, context.CancellationToken, context.Node))
            {
                if (pooled.Item.HasError)
                {
                    return;
                }

                if (pooled.Item.IsSuccess)
                {
                    var returnStatementSyntax = pooled.Item.GetValue.FirstAncestorOrSelf<ReturnStatementSyntax>();
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

            using (var pooled = ClrSetterWalker.Create(context.SemanticModel, context.CancellationToken, context.Node))
            {
                if (pooled.Item.HasError)
                {
                    return;
                }

                if (pooled.Item.IsSuccess)
                {
                    foreach (var statement in body.Statements)
                    {
                        if ((statement as ExpressionStatementSyntax)?.Expression != pooled.Item.SetValue &&
                            (statement as ExpressionStatementSyntax)?.Expression != pooled.Item.SetCurrentValue)
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