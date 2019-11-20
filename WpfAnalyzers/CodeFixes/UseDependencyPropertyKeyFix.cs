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
                    invocation is { ArgumentList: { Arguments: { } arguments } } &&
                    arguments.FirstOrDefault() is { Expression: { } dp } &&
                    semanticModel.TryGetSymbol(dp, context.CancellationToken, out var symbol) &&
                    BackingFieldOrProperty.TryCreateForDependencyProperty(symbol, out var fieldOrProperty) &&
                    DependencyProperty.TryGetDependencyPropertyKeyFieldOrProperty(fieldOrProperty, semanticModel, context.CancellationToken, out var keyField))
                {
                    if (DependencyObject.TryGetSetValueCall(invocation, semanticModel, context.CancellationToken, out _))
                    {
                        context.RegisterCodeFix(
                            invocation.ToString(),
                            (e, _) => e.ReplaceNode(arguments[0], x => keyField.CreateArgument(semanticModel, invocation.SpanStart)),
                            this.GetType().FullName,
                            diagnostic);
                    }

                    if (DependencyObject.TryGetSetCurrentValueCall(invocation, semanticModel, context.CancellationToken, out _))
                    {
                        context.RegisterCodeFix(
                            invocation.ToString(),
                            (e, _) => e.ReplaceNode(
                                invocation.Expression,
                                x => x switch
                                    {
                                        IdentifierNameSyntax id => id.WithIdentifier(SyntaxFactory.Identifier("SetValue")),
                                        MemberAccessExpressionSyntax { Name: IdentifierNameSyntax id } ma => ma.WithName(id.WithIdentifier(SyntaxFactory.Identifier("SetValue"))),
                                        _ => x,
                                    })
                                       .ReplaceNode(
                                arguments[0],
                                x => keyField.CreateArgument(semanticModel, invocation.SpanStart)),
                            this.GetType().FullName,
                            diagnostic);
                    }
                }
            }
        }
    }
}
