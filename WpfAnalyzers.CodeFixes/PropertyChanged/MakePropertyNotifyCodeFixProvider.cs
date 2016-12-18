namespace WpfAnalyzers
{
    using System;
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

                var type = (ITypeSymbol)ModelExtensions.GetDeclaredSymbol(semanticModel, typeDeclaration, context.CancellationToken);

                IMethodSymbol invoker;
                if (PropertyChanged.Helpers.PropertyChanged.TryGetInvoker(type, semanticModel, context.CancellationToken, out invoker))
                {
                    if (Property.IsMutableAutoProperty(propertyDeclaration))
                    {
                        context.RegisterCodeFix(
                            CodeAction.Create(
                                "Convert to notifying property.",
                                _ => ApplyConvertAutoPropertyFixAsync(context, syntaxRoot, propertyDeclaration, invoker),
                                nameof(ImplementINotifyPropertyChangedCodeFixProvider)),
                            diagnostic);
                    }
                }
            }
        }

        private static Task<Document> ApplyConvertAutoPropertyFixAsync(
            CodeFixContext context,
            SyntaxNode syntaxRoot,
            PropertyDeclarationSyntax propertyDeclaration,
            IMethodSymbol invoker)
        {
            var syntaxGenerator = SyntaxGenerator.GetGenerator(context.Document);
            var typeDeclaration = propertyDeclaration.FirstAncestorOrSelf<TypeDeclarationSyntax>();

            var newTypeDeclaration = typeDeclaration.TrackNodes(propertyDeclaration);
            string fieldName;
            newTypeDeclaration = newTypeDeclaration.WithBackingField(propertyDeclaration, syntaxGenerator, out fieldName);

            propertyDeclaration = newTypeDeclaration.GetCurrentNode(propertyDeclaration);
            var newPropertyDeclaration = propertyDeclaration.WithGeterReturningBackingField(syntaxGenerator, fieldName)
                                                            .WithNotifyingSetter(syntaxGenerator, fieldName, invoker);

            newTypeDeclaration = newTypeDeclaration.ReplaceNode(propertyDeclaration, newPropertyDeclaration);
            return Task.FromResult(context.Document.WithSyntaxRoot(syntaxRoot.ReplaceNode(typeDeclaration, newTypeDeclaration)));
        }
    }
}
