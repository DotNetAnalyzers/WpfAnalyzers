namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Formatting;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseSetValueFix))]
    [Shared]
    internal class UseSetValueFix : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.WPF0043DoNotUseSetCurrentValueForDataContext.Id,
            Descriptors.WPF0035ClrPropertyUseSetValueInSetter.Id);

        public override FixAllProvider? GetFixAllProvider() => null;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNodeOrAncestor(diagnostic, out InvocationExpressionSyntax? invocation))
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            "Use SetValue",
                            _ => ApplyFixAsync(context.Document, syntaxRoot, invocation),
                            this.GetType().FullName),
                        diagnostic);
                }
            }
        }

        private static Task<Document> ApplyFixAsync(Document document, SyntaxNode syntaxRoot, InvocationExpressionSyntax invocation)
        {
            return Task.FromResult(document.WithSyntaxRoot(syntaxRoot.ReplaceNode(invocation, SetValueRewriter.Default.Visit(invocation))));
        }

        private class SetValueRewriter : CSharpSyntaxRewriter
        {
            internal static readonly SetValueRewriter Default = new SetValueRewriter();
            private static readonly NameSyntax SetValueIdentifier = SyntaxFactory.ParseName("SetValue").WithAdditionalAnnotations(Formatter.Annotation);

            private SetValueRewriter()
            {
            }

            public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
            {
                if (node.Identifier.ValueText == "SetCurrentValue")
                {
                    return SetValueIdentifier;
                }

                return base.VisitIdentifierName(node);
            }
        }
    }
}
