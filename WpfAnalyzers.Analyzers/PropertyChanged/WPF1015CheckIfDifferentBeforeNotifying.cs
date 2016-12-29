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
            var invocation = (InvocationExpressionSyntax)context.Node;
            var setter = invocation.FirstAncestorOrSelf<AccessorDeclarationSyntax>();
            if (setter?.IsKind(SyntaxKind.SetAccessorDeclaration) != true)
            {
                return;
            }

            if (!IsFirstNotifyPropertyChange(invocation, context.SemanticModel, context.CancellationToken))
            {
                return;
            }

            var propertyDeclaration = setter.FirstAncestorOrSelf<PropertyDeclarationSyntax>();
            var property = context.SemanticModel.GetDeclaredSymbolSafe(propertyDeclaration, context.CancellationToken);

            IFieldSymbol backingField;
            if (!Property.TryGetBackingField(property, context.SemanticModel, context.CancellationToken, out backingField))
            {
                return;
            }

            IParameterSymbol value;
            if (Property.TryFindValue(setter, context.SemanticModel, context.CancellationToken, out value))
            {
                using (var pooledIfStatements = IfStatementWalker.Create(setter))
                {
                    foreach (var ifStatement in pooledIfStatements.Item.IfStatements)
                    {
                        if (ifStatement.SpanStart >= invocation.SpanStart)
                        {
                            continue;
                        }

                        foreach (var member in new ISymbol[] { backingField, property })
                        {
                            if (Equality.IsOperatorEquals(ifStatement.Condition, context.SemanticModel, context.CancellationToken, value, member) ||
                               IsEqualsCheck(ifStatement.Condition, context.SemanticModel, context.CancellationToken, value, member))
                            {
                                if (ifStatement.Statement.Span.Contains(invocation.Span))
                                {
                                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocation.FirstAncestorOrSelf<StatementSyntax>()?.GetLocation() ?? invocation.GetLocation()));
                                }

                                return;
                            }

                            if (Equality.IsOperatorNotEquals(ifStatement.Condition, context.SemanticModel, context.CancellationToken, value, member) ||
                                IsNegatedEqualsCheck(ifStatement.Condition, context.SemanticModel, context.CancellationToken, value, member))
                            {
                                if (!ifStatement.Statement.Span.Contains(invocation.Span))
                                {
                                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocation.FirstAncestorOrSelf<StatementSyntax>()?.GetLocation() ?? invocation.GetLocation()));
                                }

                                return;
                            }

                            if (UsesValueAndMember(ifStatement, context.SemanticModel, context.CancellationToken, value, member))
                            {
                                return;
                            }
                        }
                    }
                }
            }

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocation.FirstAncestorOrSelf<StatementSyntax>()?.GetLocation() ?? invocation.GetLocation()));
        }

        private static bool IsFirstNotifyPropertyChange(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (!PropertyChanged.IsNotifyPropertyChanged(invocation, semanticModel, cancellationToken))
            {
                return false;
            }

            var statement = invocation.FirstAncestorOrSelf<ExpressionStatementSyntax>();
            var block = statement?.FirstAncestorOrSelf<BlockSyntax>();

            if (block == null)
            {
                return false;
            }

            var index = block.Statements.IndexOf(statement);
            if (index <= 0)
            {
                return false;
            }

            return !PropertyChanged.IsNotifyPropertyChanged(block.Statements[index - 1], semanticModel, cancellationToken);
        }

        private static bool IsNegatedEqualsCheck(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken, IParameterSymbol value, ISymbol member)
        {
            var unaryExpression = expression as PrefixUnaryExpressionSyntax;
            if (unaryExpression?.IsKind(SyntaxKind.LogicalNotExpression) == true)
            {
                return IsEqualsCheck(unaryExpression.Operand, semanticModel, cancellationToken, value, member);
            }

            return false;
        }

        private static bool IsEqualsCheck(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken, IParameterSymbol value, ISymbol member)
        {
            var equals = expression as InvocationExpressionSyntax;
            if (equals == null)
            {
                return false;
            }

            if (Equality.IsObjectEquals(equals, semanticModel, cancellationToken, value, member) ||
                Equality.IsInstanceEquals(equals, semanticModel, cancellationToken, value, member) ||
                Equality.IsInstanceEquals(equals, semanticModel, cancellationToken, member, value) ||
                Equality.IsReferenceEquals(equals, semanticModel, cancellationToken, value, member) ||
                Equality.IsEqualityComparerEquals(equals, semanticModel, cancellationToken, value, member) ||
                Equality.IsNullableEquals(equals, semanticModel, cancellationToken, value, member))
            {
                return true;
            }

            return false;
        }

        private static bool UsesValueAndMember(IfStatementSyntax ifStatement, SemanticModel semanticModel, CancellationToken cancellationToken, IParameterSymbol value, ISymbol member)
        {
            var usesValue = false;
            var usesMember = false;
            using (var pooledIdentifierNames = IdentifierNameWalker.Create(ifStatement.Condition))
            {
                foreach (var identifierName in pooledIdentifierNames.Item.IdentifierNames)
                {
                    var symbol = semanticModel.GetSymbolSafe(identifierName, cancellationToken);
                    if (symbol == null)
                    {
                        continue;
                    }

                    if (symbol.Equals(value))
                    {
                        usesValue = true;
                    }

                    if (symbol.Equals(member))
                    {
                        usesMember = true;
                    }
                }
            }

            return usesMember && usesValue;
        }
    }
}