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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseDependencyPropertyKeyFix))]
    [Shared]
    internal class UseDependencyPropertyKeyFix : DocumentEditorCodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.WPF0040SetUsingDependencyPropertyKey.Id);

        protected override DocumentEditorFixAllProvider? FixAllProvider() => null;

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var document = context.Document;
            var syntaxRoot = await document.GetSyntaxRootAsync(context.CancellationToken)
                                           .ConfigureAwait(false);

            var semanticModel = await document.GetSemanticModelAsync(context.CancellationToken)
                                              .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNodeOrAncestor(diagnostic, out InvocationExpressionSyntax? invocation) &&
                    diagnostic.Properties.TryGetValue(nameof(DependencyPropertyKeyType), out var keyName))
                {
                    context.RegisterCodeFix(
                        invocation.ToString(),
                        (e, _) => e.ReplaceNode(invocation.Expression, x => SetValue(x))
                                   .ReplaceNode(invocation.ArgumentList.Arguments[0].Expression, x => PropertyKey(x)),
                        this.GetType().FullName,
                        diagnostic);

                    ExpressionSyntax PropertyKey(ExpressionSyntax old)
                    {
                        return old switch
                        {
                            IdentifierNameSyntax id => id.WithIdentifier(SyntaxFactory.Identifier(keyName)),
                            MemberAccessExpressionSyntax { Name: IdentifierNameSyntax id } ma => ma.WithName(id.WithIdentifier(SyntaxFactory.Identifier(keyName))),
                            _ => old,
                        };
                    }

                    static ExpressionSyntax SetValue(ExpressionSyntax old)
                    {
                        return old switch
                        {
                            IdentifierNameSyntax id => id.WithIdentifier(SyntaxFactory.Identifier("SetValue")),
                            MemberAccessExpressionSyntax { Name: IdentifierNameSyntax id } ma => ma.WithName(id.WithIdentifier(SyntaxFactory.Identifier("SetValue"))),
                            _ => old,
                        };
                    }
                }

            }
        }
    }
}
