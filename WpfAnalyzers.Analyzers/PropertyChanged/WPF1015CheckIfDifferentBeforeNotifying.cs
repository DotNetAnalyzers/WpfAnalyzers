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

            IdentifierNameSyntax backingField;
            FieldDeclarationSyntax field;
            var propertyDeclaration = setter.FirstAncestorOrSelf<PropertyDeclarationSyntax>();
            if (!Property.TryGetBackingField(propertyDeclaration, out backingField, out field))
            {
                return;
            }

            var method = context.SemanticModel.GetSymbolSafe(invocation, context.CancellationToken) as IMethodSymbol;
            if (method == KnownSymbol.PropertyChangedEventHandler.Invoke ||
                PropertyChanged.IsInvoker(method, context.SemanticModel, context.CancellationToken) == AnalysisResult.Yes)
            {
                IfStatementSyntax ifStatement;
                if (HasCheck(invocation, setter, backingField, propertyDeclaration, out ifStatement))
                {
                    if (ifStatement == null)
                    {
                        return;
                    }

                    var binaryExpression = ifStatement.Condition as BinaryExpressionSyntax;
                    if (binaryExpression != null)
                    {
                        if ((IsValue(binaryExpression.Left) && IsPropertyOrField(binaryExpression.Right, backingField, propertyDeclaration)) ||
                            (IsPropertyOrField(binaryExpression.Left, backingField, propertyDeclaration) && IsValue(binaryExpression.Right)))
                        {
                            if (binaryExpression.IsKind(SyntaxKind.EqualsExpression))
                            {
                                if (ifStatement.Statement.Span.Contains(invocation.Span))
                                {
                                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocation.FirstAncestorOrSelf<StatementSyntax>()?.GetLocation() ?? invocation.GetLocation()));
                                }
                            }

                            if (binaryExpression.IsKind(SyntaxKind.NotEqualsExpression))
                            {
                                if (!ifStatement.Statement.Span.Contains(invocation.Span))
                                {
                                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocation.FirstAncestorOrSelf<StatementSyntax>()?.GetLocation() ?? invocation.GetLocation()));
                                }
                            }
                        }

                        return;
                    }

                    if (IsEqualsCheck(ifStatement.Condition, backingField, propertyDeclaration))
                    {
                        if (ifStatement.Statement.Span.Contains(invocation.Span))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocation.FirstAncestorOrSelf<StatementSyntax>()?.GetLocation() ?? invocation.GetLocation()));
                        }

                        return;
                    }

                    var unaryExpressionSyntax = ifStatement.Condition as PrefixUnaryExpressionSyntax;
                    if (IsEqualsCheck(unaryExpressionSyntax?.Operand, backingField, propertyDeclaration))
                    {
                        if (!ifStatement.Statement.Span.Contains(invocation.Span))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocation.FirstAncestorOrSelf<StatementSyntax>()?.GetLocation() ?? invocation.GetLocation()));
                        }

                        return;
                    }

                    return;
                }

                context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocation.FirstAncestorOrSelf<StatementSyntax>()?.GetLocation() ?? invocation.GetLocation()));
            }
        }

        private static bool IsEqualsCheck(ExpressionSyntax expression, IdentifierNameSyntax backingField, PropertyDeclarationSyntax propertyDeclaration)
        {
            var equalsInvocation = expression as InvocationExpressionSyntax;
            if (equalsInvocation == null)
            {
                return false;
            }

            SimpleNameSyntax identifierName = equalsInvocation.Expression as IdentifierNameSyntax;
            if (identifierName == null)
            {
                var memberAccess = equalsInvocation.Expression as MemberAccessExpressionSyntax;
                identifierName = memberAccess?.Name;
            }

            if (identifierName == null)
            {
                return false;
            }

            if (identifierName.Identifier.ValueText == "Equals" || identifierName.Identifier.ValueText == "ReferenceEquals")
            {
                ArgumentSyntax _;
                if ((IsValue(equalsInvocation.Expression) && equalsInvocation.ArgumentList.Arguments.TryGetFirst(x => IsPropertyOrField(x.Expression, backingField, propertyDeclaration), out _)) ||
                    (IsPropertyOrField(equalsInvocation.Expression, backingField, propertyDeclaration) && equalsInvocation.ArgumentList.Arguments.TryGetFirst(x => IsValue(x.Expression), out _)) ||
                    (equalsInvocation.ArgumentList.Arguments.TryGetFirst(x => IsValue(x.Expression), out _) && equalsInvocation.ArgumentList.Arguments.TryGetFirst(x => IsPropertyOrField(x.Expression, backingField, propertyDeclaration), out _)))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsValue(SyntaxNode node)
        {
            var nameSyntax = node as SimpleNameSyntax;
            if (nameSyntax == null)
            {
                return false;
            }

            return nameSyntax.Identifier.ValueText == "value";
        } 

        private static bool IsPropertyOrField(SyntaxNode node, IdentifierNameSyntax backingField, PropertyDeclarationSyntax propertyDeclaration)
        {
            var nameSyntax = node as SimpleNameSyntax;
            if (nameSyntax == null)
            {
                var memberAccess = node as MemberAccessExpressionSyntax;
                nameSyntax = memberAccess?.Expression as IdentifierNameSyntax;
                if (nameSyntax == null)
                {
                    if (!(memberAccess?.Expression is ThisExpressionSyntax))
                    {
                        return false;
                    }
                }

                nameSyntax = memberAccess.Name;
            }

            if (nameSyntax == null)
            {
                return false;
            }

            return nameSyntax.Identifier.ValueText == backingField.Identifier.ValueText ||
                   nameSyntax.Identifier.ValueText == propertyDeclaration.Identifier.ValueText;
        }

        private static bool HasCheck(InvocationExpressionSyntax invocation, AccessorDeclarationSyntax setter, IdentifierNameSyntax backingField, PropertyDeclarationSyntax propertyDeclaration, out IfStatementSyntax ifStatement)
        {
            ifStatement = null;
            bool result = false;
            using (var pooledIfStatements = IfStatementWalker.Create(setter))
            {
                foreach (var statement in pooledIfStatements.Item.IfStatements)
                {
                    if (statement.SpanStart < invocation.SpanStart)
                    {
                        bool usesValue = false;
                        bool usesField = false;
                        using (var pooledIdentifierNames = IdentifierNameWalker.Create(statement.Condition))
                        {
                            foreach (var identifierName in pooledIdentifierNames.Item.IdentifierNames)
                            {
                                if (IsValue(identifierName))
                                {
                                    usesValue = true;
                                }

                                if (IsPropertyOrField(identifierName, backingField, propertyDeclaration))
                                {
                                    usesField = true;
                                }
                            }
                        }

                        if (usesField && usesValue)
                        {
                            if (!result)
                            {
                                ifStatement = statement;
                            }
                            else
                            {
                                ifStatement = null;
                            }

                            result = true;
                        }
                    }
                }
            }

            return result;
        }
    }
}