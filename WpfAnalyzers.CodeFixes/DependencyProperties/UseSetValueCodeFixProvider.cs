namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Formatting;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseSetValueCodeFixProvider))]
    [Shared]
    internal class UseSetValueCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(WPF0043DontUseSetCurrentValueForDataContext.DiagnosticId);

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                var token = syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start);
                if (string.IsNullOrEmpty(token.ValueText))
                {
                    continue;
                }

                var invocation = syntaxRoot.FindNode(diagnostic.Location.SourceSpan).FirstAncestorOrSelf<InvocationExpressionSyntax>();
                if (invocation == null || invocation.IsMissing)
                {
                    continue;
                }

                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Use SetValue",
                        _ => ApplyFixAsync(context.Document, syntaxRoot, invocation),
                        this.GetType().FullName),
                    diagnostic);
            }
        }

        private static Task<Document> ApplyFixAsync(Document document, SyntaxNode syntaxRoot, InvocationExpressionSyntax invocation)
        {
            return Task.FromResult(document.WithSyntaxRoot(syntaxRoot.ReplaceNode(invocation, SetValueRewriter.Default.Visit(invocation))));
        }

        private class SetValueRewriter : CSharpSyntaxRewriter
        {
            public static readonly SetValueRewriter Default = new SetValueRewriter();
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