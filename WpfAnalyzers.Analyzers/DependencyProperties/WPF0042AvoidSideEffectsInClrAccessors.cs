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
        private const string Title = "Avoid side effects in CLR accessors.";
        private const string MessageFormat = "Avoid side effects in CLR accessors.";
        private const string Description = "Avoid side effects in CLR accessors.";
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
            context.RegisterSyntaxNodeAction(HandleMethod, SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(HandleGetter, SyntaxKind.GetAccessorDeclaration);
            context.RegisterSyntaxNodeAction(HandleSetter, SyntaxKind.SetAccessorDeclaration);
        }

        private static void HandleMethod(SyntaxNodeAnalysisContext context)
        {
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

            using (var getterWalker = ClrGetterWalker.Create(context.SemanticModel, context.CancellationToken, context.Node))
            {
                if (getterWalker.HasError)
                {
                    return;
                }

                if (getterWalker.IsSuccess)
                {
                    var returnStatementSyntax = getterWalker.GetValue.FirstAncestorOrSelf<ReturnStatementSyntax>();
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

            using (var getterWalker = ClrSetterWalker.Create(context.SemanticModel, context.CancellationToken, context.Node))
            {
                if (getterWalker.HasError)
                {
                    return;
                }

                if (getterWalker.IsSuccess)
                {
                    foreach (var statement in body.Statements)
                    {
                        if ((statement as ExpressionStatementSyntax)?.Expression != getterWalker.SetValue &&
                            (statement as ExpressionStatementSyntax)?.Expression != getterWalker.SetCurrentValue)
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