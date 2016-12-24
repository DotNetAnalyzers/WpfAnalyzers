namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;

    using WpfAnalyzers.PropertyChanged;
    using WpfAnalyzers.PropertyChanged.Helpers;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MakePropertyNotifyCodeFixProvider))]
    [Shared]
    internal class MakePropertyNotifyCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(WPF1010MutablePublicPropertyShouldNotify.DiagnosticId);

        /// <inheritdoc/>
        //public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

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

                var propertyDeclaration = syntaxRoot.FindNode(diagnostic.Location.SourceSpan).FirstAncestorOrSelf<PropertyDeclarationSyntax>();
                var typeDeclaration = propertyDeclaration?.FirstAncestorOrSelf<TypeDeclarationSyntax>();
                if (typeDeclaration == null)
                {
                    continue;
                }

                var property = semanticModel.GetDeclaredSymbolSafe(propertyDeclaration, context.CancellationToken);
                var type = semanticModel.GetDeclaredSymbolSafe(typeDeclaration, context.CancellationToken);

                IMethodSymbol invoker;
                if (PropertyChanged.Helpers.PropertyChanged.TryGetInvoker(type, semanticModel, context.CancellationToken, out invoker) &&
                    invoker.Parameters[0].Type == KnownSymbol.String)
                {
                    if (Property.IsMutableAutoProperty(propertyDeclaration))
                    {
                        context.RegisterCodeFix(
                            CodeAction.Create(
                                "Convert to notifying property.",
                                _ => ApplyConvertAutoPropertyFixAsync(context, syntaxRoot, propertyDeclaration, property, invoker),
                                 this.GetType().FullName),
                            diagnostic);
                    }

                    string fieldName;
                    ExpressionStatementSyntax assignStatement;
                    if (IsSimpleAssignmentOnly(propertyDeclaration, semanticModel, context.CancellationToken, out assignStatement, out fieldName))
                    {
                        context.RegisterCodeFix(
                            CodeAction.Create(
                                "Convert to notifying property.",
                                _ => ApplyConvertAutoPropertyFixAsync(context, syntaxRoot, propertyDeclaration, property, assignStatement, fieldName, invoker),
                                 this.GetType().FullName),
                            diagnostic);
                    }
                }
            }
        }

        private static bool IsSimpleAssignmentOnly(PropertyDeclarationSyntax propertyDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken, out ExpressionStatementSyntax assignStatement, out string fieldName)
        {
            fieldName = null;
            assignStatement = null;
            AccessorDeclarationSyntax setter;
            if (!propertyDeclaration.TryGetSetAccessorDeclaration(out setter) ||
                setter.Body == null ||
                setter.Body.Statements.Count != 1)
            {
                return false;
            }

            assignStatement = setter.Body.Statements[0] as ExpressionStatementSyntax;
            var assignment = assignStatement?.Expression as AssignmentExpressionSyntax;
            if (assignment == null || (assignment.Right as IdentifierNameSyntax)?.Identifier.ValueText != "value")
            {
                return false;
            }

            var fieldSymbol = semanticModel.GetSymbolSafe(assignment.Left, cancellationToken) as IFieldSymbol;
            if (fieldSymbol == null)
            {
                return false;
            }

            fieldName = fieldSymbol.Name;
            return true;
        }

        private static Task<Document> ApplyConvertAutoPropertyFixAsync(
            CodeFixContext context,
            SyntaxNode syntaxRoot,
            PropertyDeclarationSyntax propertyDeclaration,
            IPropertySymbol property,
            IMethodSymbol invoker)
        {
            var syntaxGenerator = SyntaxGenerator.GetGenerator(context.Document);
            var typeDeclaration = propertyDeclaration.FirstAncestorOrSelf<TypeDeclarationSyntax>();

            var newTypeDeclaration = typeDeclaration.TrackNodes(propertyDeclaration);
            string fieldName;
            newTypeDeclaration = newTypeDeclaration.WithBackingField(propertyDeclaration, syntaxGenerator, out fieldName);

            propertyDeclaration = newTypeDeclaration.GetCurrentNode(propertyDeclaration);
            var newPropertyDeclaration = propertyDeclaration.WithGetterReturningBackingField(syntaxGenerator, fieldName)
                                                            .WithNotifyingSetter(syntaxGenerator, property, fieldName, invoker);

            newTypeDeclaration = newTypeDeclaration.ReplaceNode(propertyDeclaration, newPropertyDeclaration);
            return Task.FromResult(context.Document.WithSyntaxRoot(syntaxRoot.ReplaceNode(typeDeclaration, newTypeDeclaration)));
        }

        private static Task<Document> ApplyConvertAutoPropertyFixAsync(
            CodeFixContext context,
            SyntaxNode syntaxRoot,
            PropertyDeclarationSyntax propertyDeclaration,
            IPropertySymbol property,
            ExpressionStatementSyntax assignStatement,
            string fieldName,
            IMethodSymbol invoker)
        {
            var syntaxGenerator = SyntaxGenerator.GetGenerator(context.Document);
            var typeDeclaration = propertyDeclaration.FirstAncestorOrSelf<TypeDeclarationSyntax>();

            var newPropertyDeclaration = propertyDeclaration.WithGetterReturningBackingField(syntaxGenerator, fieldName)
                                                            .WithNotifyingSetter(syntaxGenerator, property, assignStatement, fieldName, invoker);

            var newTypeDeclaration = typeDeclaration.ReplaceNode(propertyDeclaration, newPropertyDeclaration);
            return Task.FromResult(context.Document.WithSyntaxRoot(syntaxRoot.ReplaceNode(typeDeclaration, newTypeDeclaration)));
        }
    }
}
