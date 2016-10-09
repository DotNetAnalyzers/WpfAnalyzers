namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseSetCurrentValueCodeFixProvider))]
    [Shared]
    internal class UseSetCurrentValueCodeFixProvider : CodeFixProvider
    {
        private static readonly IdentifierNameSyntax SetCurrentValueIdentifier = SyntaxFactory.IdentifierName(Names.SetCurrentValue);

        private static readonly ExpressionSyntax ThisSetCurrentValueExpression =
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.ThisExpression(),
                SyntaxFactory.Token(SyntaxKind.DotToken),
                SetCurrentValueIdentifier);

        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(WPF0041SetMutableUsingSetCurrentValue.DiagnosticId);

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        "SetCurrentValue()",
                        _ => ApplyFixAsync(context, diagnostic),
                        nameof(MakeFieldStaticReadonlyCodeFixProvider)),
                    diagnostic);
            }

            return FinishedTasks.Task;
        }

        private static async Task<Document> ApplyFixAsync(CodeFixContext context, Diagnostic diagnostic)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                              .ConfigureAwait(false);
            var assignment = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                       .FirstAncestorOrSelf<AssignmentExpressionSyntax>();
            if (assignment != null)
            {
                var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
                ArgumentListSyntax arguments;
                if (!TryCreateArguments(assignment, semanticModel, context.CancellationToken, out arguments))
                {
                    return context.Document;
                }

                ExpressionSyntax call;
                if (!TryCreateCallExpression(assignment, out call))
                {
                    return context.Document;
                }

                var invocationExpression = SyntaxFactory.InvocationExpression(call, arguments);
                var updated = syntaxRoot.ReplaceNode(assignment, invocationExpression);
                return context.Document.WithSyntaxRoot(updated);
            }

            var invocation = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                           .FirstAncestorOrSelf<InvocationExpressionSyntax>();
            if (invocation != null)
            {
                var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
                ArgumentSyntax property;
                ArgumentSyntax value;
                if (!invocation.TryGetSetValueArguments(semanticModel, context.CancellationToken, out property, out value))
                {
                    return context.Document;
                }

                ExpressionSyntax setCurrentValue;
                if (TryCreateCallExpression(invocation, out setCurrentValue))
                {
                    var updated = syntaxRoot.ReplaceNode(invocation.Expression, setCurrentValue);
                    return context.Document.WithSyntaxRoot(updated);
                }

                return context.Document;
            }

            return context.Document;
        }

        private static bool TryCreateCallExpression(AssignmentExpressionSyntax assignment, out ExpressionSyntax result)
        {
            result = null;
            var memberAccess = assignment.Left as MemberAccessExpressionSyntax;
            if (memberAccess != null)
            {
                if (memberAccess.Expression.IsKind(SyntaxKind.ThisExpression))
                {
                    result = ThisSetCurrentValueExpression;
                    return true;
                }

                result = memberAccess.WithName(SetCurrentValueIdentifier);
                return true;
            }

            if (assignment.Left.IsKind(SyntaxKind.IdentifierName))
            {
                result = SetCurrentValueIdentifier;
                return true;
            }

            return false;
        }

        private static bool TryCreateCallExpression(InvocationExpressionSyntax invocation, out ExpressionSyntax result)
        {
            result = null;
            var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
            if (memberAccess != null)
            {
                if (memberAccess.Expression.IsKind(SyntaxKind.ThisExpression))
                {
                    result = ThisSetCurrentValueExpression;
                    return true;
                }

                result = memberAccess.WithName(SetCurrentValueIdentifier);
                return true;
            }

            if (invocation.Expression.IsKind(SyntaxKind.IdentifierName))
            {
                result = SetCurrentValueIdentifier;
                return true;
            }

            return false;
        }

        private static bool TryCreateArguments(AssignmentExpressionSyntax assignment, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentListSyntax result)
        {
            result = null;
            var property = semanticModel.GetSymbolInfo(assignment.Left, cancellationToken).Symbol as IPropertySymbol;
            if (property == null || property.IsIndexer)
            {
                return false;
            }

            var classDeclaration = assignment.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            if (classDeclaration == null)
            {
                return false;
            }

            AccessorDeclarationSyntax setter;
            if (!property.TryGetSetterSyntax(out setter))
            {
                if (property.ContainingType.IsAssignableToDependencyObject())
                {
                    IFieldSymbol field;
                    if (property.TryGetMutableDependencyPropertyField(out field))
                    {
                        var type = (ITypeSymbol)semanticModel.GetDeclaredSymbol(classDeclaration);
                        result = CreateArguments(field, type, assignment);
                        return true;
                    }
                }

                return false;
            }

            FieldDeclarationSyntax dependencyProperty;
            if (setter.TryGetDependencyPropertyFromSetter(out dependencyProperty))
            {
                FieldDeclarationSyntax temp;
                if (dependencyProperty.TryGetDependencyPropertyKey(out temp))
                {
                    return false;
                }

                var field = dependencyProperty.FieldSymbol(semanticModel);
                var type = (ITypeSymbol)semanticModel.GetDeclaredSymbol(classDeclaration);
                result = CreateArguments(field, type, assignment);
                return true;
            }

            return false;
        }

        private static ArgumentListSyntax CreateArguments(IFieldSymbol field, ITypeSymbol type, AssignmentExpressionSyntax assignment)
        {
            // a hack here, feels like the semantic model should be able to answer this.
            var identifier = field.ContainingType.IsAssignableTo(type)
                ? SyntaxFactory.IdentifierName(field.Name)
                : SyntaxFactory.IdentifierName($"{field.ContainingType.Name}.{field.Name}");
            return SyntaxFactory.ArgumentList()
                                     .AddArguments(
                                         SyntaxFactory.Argument(identifier),
                                         SyntaxFactory.Argument(assignment.Right))
                                     .NormalizeWhitespace();
        }
    }
}
