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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseSetValueFix))]
    [Shared]
    internal class UseSetValueFix : DocumentEditorCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.WPF0043DoNotUseSetCurrentValueForDataContext.Id,
            Descriptors.WPF0035ClrPropertyUseSetValueInSetter.Id);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                             .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot is { } &&
                    syntaxRoot.TryFindNodeOrAncestor(diagnostic, out InvocationExpressionSyntax? invocation) &&
                    IdentifierName(invocation.Expression) is { } methodIdentifier &&
                    semanticModel is { } &&
                    invocation is { ArgumentList: { Arguments: { Count: 2 } arguments } } &&
                    semanticModel.TryGetSymbol(arguments[0].Expression, context.CancellationToken, out var symbol) &&
                    BackingFieldOrProperty.TryCreateForDependencyProperty(symbol, out var backing))
                {
                    if (DependencyProperty.TryGetDependencyPropertyKeyFieldOrProperty(backing, semanticModel, context.CancellationToken, out var key))
                    {
                        if (IdentifierName(arguments[0].Expression) is { } propertyIdentifier)
                        {
                            context.RegisterCodeFix(
                                "Use SetValue",
                                e => e.ReplaceNode(
                                    methodIdentifier,
                                    x => x.WithIdentifier(SyntaxFactory.Identifier("SetValue")))
                                      .ReplaceNode(
                                          propertyIdentifier,
                                          x => x.WithIdentifier(SyntaxFactory.Identifier(key.Name))),
                                "Use SetValue",
                                diagnostic);
                        }
                    }
                    else
                    {
                        context.RegisterCodeFix(
                            "Use SetValue",
                            e => e.ReplaceNode(
                                methodIdentifier,
                                x => x.WithIdentifier(SyntaxFactory.Identifier("SetValue"))),
                            "Use SetValue",
                            diagnostic);
                    }
                }

                static IdentifierNameSyntax? IdentifierName(ExpressionSyntax? expression)
                {
                    return expression switch
                    {
                        IdentifierNameSyntax identifierName => identifierName,
                        MemberAccessExpressionSyntax { Name: IdentifierNameSyntax identifierName } => identifierName,
                        _ => null,
                    };
                }
            }
        }
    }
}
