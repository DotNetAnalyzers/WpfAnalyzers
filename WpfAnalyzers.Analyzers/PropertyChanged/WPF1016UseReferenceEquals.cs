namespace WpfAnalyzers.PropertyChanged
{
    using System.Collections.Immutable;
    using System.Threading;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    using WpfAnalyzers.PropertyChanged.Helpers;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF1016UseReferenceEquals : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF1016";
        private const string Title = "Check if value is different using ReferenceEquals before notifying.";
        private const string MessageFormat = "Check if value is different using ReferenceEquals before notifying.";
        private const string Description = Title;
        private static readonly string HelpLink = WpfAnalyzers.HelpLink.ForId(DiagnosticId);

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            AnalyzerCategory.PropertyChanged,
            DiagnosticSeverity.Hidden,
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
            context.RegisterSyntaxNodeAction(HandleInvocation, SyntaxKind.IfStatement);
        }

        private static void HandleInvocation(SyntaxNodeAnalysisContext context)
        {
            var ifStatement = (IfStatementSyntax)context.Node;
            if (ifStatement?.Condition == null)
            {
                return;
            }

            var setter = ifStatement.FirstAncestorOrSelf<AccessorDeclarationSyntax>();
            if (setter?.IsKind(SyntaxKind.SetAccessorDeclaration) != true)
            {
                return;
            }

            if (!Notifies(setter, context.SemanticModel, context.CancellationToken))
            {
                return;
            }

            var propertyDeclaration = setter.FirstAncestorOrSelf<PropertyDeclarationSyntax>();
            var property = context.SemanticModel.GetDeclaredSymbolSafe(propertyDeclaration, context.CancellationToken);

            if (property == null ||
                property.Type.IsValueType ||
                property.Type == KnownSymbol.String)
            {
                return;
            }

            IFieldSymbol backingField;
            if (!Property.TryGetBackingField(property, context.SemanticModel, context.CancellationToken, out backingField))
            {
                return;
            }

            IParameterSymbol value;
            if (Property.TryFindValue(setter, context.SemanticModel, context.CancellationToken, out value))
            {
                foreach (var member in new ISymbol[] { backingField, property })
                {
                    if (Equality.IsReferenceEquals(ifStatement.Condition, context.SemanticModel, context.CancellationToken, value, member) ||
                        IsNegatedReferenceEqualsCheck(ifStatement.Condition, context.SemanticModel, context.CancellationToken, value, member))
                    {
                        return;
                    }
                }
            }

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, ifStatement.GetLocation()));
        }

        private static bool Notifies(AccessorDeclarationSyntax setter, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            using (var pooled = InvocationWalker.Create(setter))
            {
                foreach (var invocation in pooled.Item.Invocations)
                {
                    if (PropertyChanged.IsNotifyPropertyChanged(invocation, semanticModel, cancellationToken))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsNegatedReferenceEqualsCheck(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken, IParameterSymbol value, ISymbol member)
        {
            var unaryExpression = expression as PrefixUnaryExpressionSyntax;
            if (unaryExpression?.IsKind(SyntaxKind.LogicalNotExpression) == true)
            {
                return Equality.IsReferenceEquals(unaryExpression.Operand, semanticModel, cancellationToken, value, member);
            }

            return false;
        }
    }
}