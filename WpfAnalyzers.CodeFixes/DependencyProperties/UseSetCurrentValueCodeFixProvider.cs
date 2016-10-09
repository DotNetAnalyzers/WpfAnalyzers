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
    using Microsoft.CodeAnalysis.Simplification;

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

            AccessorDeclarationSyntax setter;
            if (!property.TryGetSetterSyntax(out setter))
            {
                if (property.ContainingType.IsAssignableToDependencyObject())
                {
                    IFieldSymbol field;
                    if (property.TryGetMutableDependencyPropertyField(out field))
                    {
                        return TryCreateArguments(field, assignment, semanticModel, cancellationToken, out result);
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

                IFieldSymbol field;
                if (dependencyProperty.TryGetFieldSymbol(semanticModel, cancellationToken, out field))
                {

                    return TryCreateArguments(field, assignment, semanticModel, cancellationToken, out result);
                }
            }

            return false;
        }

        private static bool TryCreateArguments(IFieldSymbol dependencyProperty, AssignmentExpressionSyntax assignment, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentListSyntax result)
        {
            result = null;
            var classDeclaration = assignment.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            if (classDeclaration == null)
            {
                return false;
            }

            var containingType = (ITypeSymbol)semanticModel.GetDeclaredSymbol(classDeclaration);

            var assignedType = semanticModel.GetTypeInfo(assignment.Left, cancellationToken).Type;

            ArgumentSyntax value;
            if (assignedType.IsRepresentationConservingConversion(assignment.Right, semanticModel, cancellationToken))
            {
                value = SyntaxFactory.Argument(assignment.Right);
            }
            else
            {
                value = SyntaxFactory.Argument(SyntaxFactory.CastExpression(SyntaxFactory.ParseTypeName(assignedType.ToMinimalDisplayString(semanticModel, 0)), assignment.Right));
            }

            if (assignment.Left.IsKind(SyntaxKind.IdentifierName))
            {
                result = SyntaxFactory.ArgumentList()
                    .AddArguments(
                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName(dependencyProperty.Name)),
                        value)
                    .NormalizeWhitespace();
                return true;
            }

            var memberAccess = assignment.Left as MemberAccessExpressionSyntax;
            if (memberAccess?.Expression != null)
            {
                var declaringType = semanticModel.GetTypeInfo(memberAccess.Expression, cancellationToken)
                                              .Type;

                var identifier = containingType.IsAssignableTo(declaringType)
                                     ? SyntaxFactory.IdentifierName(dependencyProperty.Name)
                                     : SyntaxFactory.IdentifierName($"{declaringType.ToMinimalDisplayString(semanticModel, 0)}.{dependencyProperty.Name}");

                identifier = identifier.WithAdditionalAnnotations(Simplifier.Annotation);
                result = SyntaxFactory.ArgumentList()
                                    .AddArguments(
                                        SyntaxFactory.Argument(identifier),
                                        value)
                                    .NormalizeWhitespace();
                return true;
            }

            return false;
        }
    }
}
