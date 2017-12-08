namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class PropertyDeclarationAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            WPF0003ClrPropertyShouldMatchRegisteredName.Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Handle, SyntaxKind.PropertyDeclaration);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (!context.ContainingSymbol.IsStatic &&
                context.ContainingSymbol.ContainingType.Is(KnownSymbol.DependencyObject) &&
                context.Node is PropertyDeclarationSyntax propertyDeclaration &&
                PropertyDeclarationWalker.TryGetCalls(propertyDeclaration, out var getCall, out var setCall) &&
                ClrProperty.TryGetRegisterField(propertyDeclaration, context.SemanticModel, context.CancellationToken, out var fieldOrProperty))
            {
                if (DependencyProperty.TryGetRegisteredName(fieldOrProperty, context.SemanticModel, context.CancellationToken, out var registeredName) &&
                    registeredName != context.ContainingSymbol.Name)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            WPF0003ClrPropertyShouldMatchRegisteredName.Descriptor,
                            propertyDeclaration.Identifier.GetLocation(),
                            context.ContainingSymbol.Name,
                            registeredName));
                }
            }
        }

        private class PropertyDeclarationWalker : PooledWalker<PropertyDeclarationWalker>
        {
            private InvocationExpressionSyntax getCall;
            private InvocationExpressionSyntax setCall;

            public static bool TryGetCalls(PropertyDeclarationSyntax eventDeclaration, out InvocationExpressionSyntax getCall, out InvocationExpressionSyntax setCall)
            {
                using (var walker = BorrowAndVisit(eventDeclaration, () => new PropertyDeclarationWalker()))
                {
                    getCall = walker.getCall;
                    setCall = walker.setCall;
                    return getCall != null || setCall != null;
                }
            }

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                if (node.TryGetInvokedMethodName(out var name) &&
                    (name == "SetValue" || name == "SetCurrentValue" || name == "GetValue") &&
                    node.FirstAncestor<AccessorDeclarationSyntax>() is AccessorDeclarationSyntax accessor)
                {
                    if (accessor.IsKind(SyntaxKind.GetAccessorDeclaration))
                    {
                        this.getCall = node;
                    }
                    else if (accessor.IsKind(SyntaxKind.SetAccessorDeclaration))
                    {
                        this.setCall = node;
                    }
                }

                base.VisitInvocationExpression(node);
            }

            protected override void Clear()
            {
                this.getCall = null;
                this.setCall = null;
            }
        }
    }
}