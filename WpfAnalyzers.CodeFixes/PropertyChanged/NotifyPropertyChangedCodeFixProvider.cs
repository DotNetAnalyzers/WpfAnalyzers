namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;
    using WpfAnalyzers.PropertyChanged;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NotifyPropertyChangedCodeFixProvider))]
    [Shared]
    internal class NotifyPropertyChangedCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(WPF1012NotifyWhenPropertyChanges.DiagnosticId);

        /// <inheritdoc/>
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);

            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                             .ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                var token = syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start);
                if (string.IsNullOrEmpty(token.ValueText))
                {
                    continue;
                }

                var assignment = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                           .FirstAncestorOrSelf<AssignmentExpressionSyntax>();
                var typeDeclaration = assignment?.FirstAncestorOrSelf<TypeDeclarationSyntax>();
                if (typeDeclaration == null)
                {
                    continue;
                }

                string property;
                if (!diagnostic.Properties.TryGetValue(WPF1012NotifyWhenPropertyChanges.PropertyNameKey, out property))
                {
                    continue;
                }

                var type = semanticModel.GetDeclaredSymbolSafe(typeDeclaration, context.CancellationToken);

                IMethodSymbol invoker;
                if (PropertyChanged.Helpers.PropertyChanged.TryGetInvoker(type, semanticModel, context.CancellationToken, out invoker) &&
                    invoker.Parameters[0].Type == KnownSymbol.String)
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            "Notify property change.",
                            _ => ApplyNotifyPropertyChangeFixAsync(context, syntaxRoot, assignment, property, invoker),
                            nameof(ImplementINotifyPropertyChangedCodeFixProvider)),
                        diagnostic);
                }
            }
        }

        private static Task<Document> ApplyNotifyPropertyChangeFixAsync(CodeFixContext context, SyntaxNode syntaxRoot, AssignmentExpressionSyntax assignment, string property, IMethodSymbol invoker)
        {
            var syntaxGenerator = SyntaxGenerator.GetGenerator(context.Document);
            var usesUnderscoreNames = assignment.FirstAncestorOrSelf<TypeDeclarationSyntax>().UsesUnderscoreNames();
            var onPropertyChanged = syntaxGenerator.OnPropertyChanged(property, false, usesUnderscoreNames, invoker);
            var previousStatement = assignment.FirstAncestorOrSelf<StatementSyntax>();
            var block = previousStatement.FirstAncestorOrSelf<BlockSyntax>();
            var index = block.Statements.IndexOf(previousStatement);
            for (var i = index + 1; i < block.Statements.Count; i++)
            {
                var statement = block.Statements[i] as ExpressionStatementSyntax;
                var invocation = statement?.Expression as InvocationExpressionSyntax;
                if (invocation == null)
                {
                    break;
                }

                var identifierName = invocation.Expression as IdentifierNameSyntax;
                if (identifierName == null)
                {
                    var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
                    if (!(memberAccess?.Expression is ThisExpressionSyntax))
                    {
                        break;
                    }

                    identifierName = memberAccess.Name as IdentifierNameSyntax;
                }

                if (identifierName == null)
                {
                    break;
                }

                if (identifierName.Identifier.ValueText == invoker.Name)
                {
                    previousStatement = statement;
                }
            }

            return Task.FromResult(context.Document.WithSyntaxRoot(syntaxRoot.InsertNodesAfter(previousStatement, new[] { onPropertyChanged })));
        }
    }
}