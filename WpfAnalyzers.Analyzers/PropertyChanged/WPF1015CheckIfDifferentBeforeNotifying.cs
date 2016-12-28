namespace WpfAnalyzers.PropertyChanged
{
    using System.Collections.Immutable;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    using WpfAnalyzers.PropertyChanged.Helpers;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF1015CheckIfDifferentBeforeNotifying : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF1015";

        private const string Title = "Check if value is different before notifying.";

        private const string MessageFormat = "Check if value is different before notifying.";

        private const string Description = Title;

        private static readonly string HelpLink = WpfAnalyzers.HelpLink.ForId(DiagnosticId);

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            AnalyzerCategory.PropertyChanged,
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
            context.RegisterSyntaxNodeAction(HandleInvocation, SyntaxKind.InvocationExpression);
        }

        private static void HandleInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            var setter = invocation.FirstAncestorOrSelf<AccessorDeclarationSyntax>();
            if (setter?.IsKind(SyntaxKind.SetAccessorDeclaration) != true)
            {
                return;
            }

            var method = context.SemanticModel.GetSymbolSafe(invocation, context.CancellationToken) as IMethodSymbol;

            IdentifierNameSyntax fieldIdentifier;
            FieldDeclarationSyntax field;
            if (!Property.TryGetBackingField(setter.FirstAncestorOrSelf<PropertyDeclarationSyntax>(), out fieldIdentifier, out field))
            {
                return;
            }

            if (method == KnownSymbol.PropertyChangedEventHandler.Invoke ||
                PropertyChanged.IsInvoker(method, context.SemanticModel, context.CancellationToken) == PropertyChanged.InvokesPropertyChanged.Yes)
            {
                using (var pooledIfStatements = IfStatementWalker.Create(setter))
                {
                    foreach (var ifStatement in pooledIfStatements.Item.IfStatements)
                    {
                        if (ifStatement.SpanStart < invocation.SpanStart)
                        {
                            bool usesValue = false;
                            bool usesField = false;
                            using (var pooledIdentifierNames = IdentifierNameWalker.Create(ifStatement.Condition))
                            {
                                foreach (var identifierName in pooledIdentifierNames.Item.IdentifierNames)
                                {
                                    if (identifierName.Identifier.ValueText == "value")
                                    {
                                        usesValue = true;
                                    }

                                    if (identifierName.Identifier.ValueText == fieldIdentifier.Identifier.ValueText)
                                    {
                                        usesField = true;
                                    }
                                }
                            }

                            if (usesField && usesValue)
                            {
                                return;
                            }
                        }
                    }
                }

                context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocation.FirstAncestorOrSelf<StatementSyntax>()?.GetLocation() ?? invocation.GetLocation()));
            }
        }
    }
}