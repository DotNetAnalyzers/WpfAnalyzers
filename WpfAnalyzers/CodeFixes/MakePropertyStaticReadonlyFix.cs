namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MakePropertyStaticReadonlyFix))]
    [Shared]
    internal class MakePropertyStaticReadonlyFix : DocumentEditorCodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.WPF0030BackingFieldShouldBeStaticReadonly.Id,
            WPF0107BackingMemberShouldBeStaticReadonly.Descriptor.Id,
            WPF0123BackingMemberShouldBeStaticReadonly.Descriptor.Id);

        /// <inheritdoc/>
        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNodeOrAncestor(diagnostic, out PropertyDeclarationSyntax propertyDeclaration))
                {
                    context.RegisterCodeFix(
                        "Make static readonly",
                        (e, _) => e.ReplaceNode(propertyDeclaration, p => ToStaticGetOnly(p)),
                        this.GetType(),
                        diagnostic);
                }
            }
        }

        private static PropertyDeclarationSyntax ToStaticGetOnly(PropertyDeclarationSyntax property)
        {
            if (!property.Modifiers.Any(SyntaxKind.StaticKeyword))
            {
                property = property.WithModifiers(property.Modifiers.WithStatic());
            }

            if (property.TryGetSetter(out var setter) &&
                setter.Body == null)
            {
                return property.RemoveNode(setter, SyntaxRemoveOptions.KeepNoTrivia);
            }

            if (property.ExpressionBody != null)
            {
                return property
                       .WithInitializer(SyntaxFactory.EqualsValueClause(property.ExpressionBody.Expression))
                       .WithExpressionBody(null)
                       .WithAccessorList(
                           SyntaxFactory.AccessorList(
                               SyntaxFactory.SingletonList(
                                   SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                                .WithSemicolonToken(
                                                    SyntaxFactory.Token(SyntaxKind.SemicolonToken)))));
            }

            return property;
        }
    }
}
