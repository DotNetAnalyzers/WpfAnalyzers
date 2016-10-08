namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseDependencyPropertyKeyCodeFixProvider))]
    [Shared]
    internal class UseDependencyPropertyKeyCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(WPF0040SetUsingDependencyPropertyKey.DiagnosticId);

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                          "Use DependencyPropertyKey when setting a readonly property.",
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
            var invocation = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                       .FirstAncestorOrSelf<InvocationExpressionSyntax>();
            if (invocation == null || invocation.IsMissing)
            {
                return context.Document;
            }

            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

            SyntaxNode updated = invocation.WithExpression(SetValueExpression(invocation.Expression))
                                           .WithArgumentList(UpdateArgumentList(invocation.ArgumentList, semanticModel, context.CancellationToken));

            return context.Document.WithSyntaxRoot(syntaxRoot.ReplaceNode(invocation, updated));
        }

        private static ExpressionSyntax SetValueExpression(ExpressionSyntax old)
        {
            var identifierNameSyntax = old as IdentifierNameSyntax;
            if (identifierNameSyntax != null)
            {
                return identifierNameSyntax.Identifier.ValueText == "SetCurrentValue"
                           ? identifierNameSyntax.WithIdentifier(SyntaxFactory.Identifier("SetValue"))
                           : identifierNameSyntax;
            }

            var memberAccessExpressionSyntax = old as MemberAccessExpressionSyntax;
            if (memberAccessExpressionSyntax != null)
            {
                var newName = SetValueExpression(memberAccessExpressionSyntax.Name) as SimpleNameSyntax;
                return memberAccessExpressionSyntax.WithName(newName);
            }

            return old;
        }

        private static ArgumentListSyntax UpdateArgumentList(ArgumentListSyntax argumentList, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            var argument = argumentList.Arguments[0];
            var dp = semanticModel.GetSymbolInfo(argument.Expression, cancellationToken);
            if (dp.Symbol.DeclaringSyntaxReferences.Length != 1)
            {
                return argumentList;
            }

            var declarator = dp.Symbol
                               .DeclaringSyntaxReferences[0]
                               .GetSyntax() as VariableDeclaratorSyntax;
            if (declarator == null)
            {
                return argumentList;
            }

            var member = declarator.Initializer.Value as MemberAccessExpressionSyntax;
            if (member?.Expression != null && member.IsDependencyPropertyKeyProperty())
            {
                var updated = argument.WithExpression(member.Expression);
                return argumentList.WithArguments(argumentList.Arguments.Replace(argument, updated));
            }

            return argumentList;
        }
    }
}