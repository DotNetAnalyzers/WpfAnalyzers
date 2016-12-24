namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Simplification;
    using WpfAnalyzers.PropertyChanged;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseCallerMemberNameCodeFixProvider))]
    [Shared]
    internal class UseCallerMemberNameCodeFixProvider : CodeFixProvider
    {
        private static readonly AttributeListSyntax CallerMemberName = SyntaxFactory.AttributeList(
            SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Attribute(SyntaxFactory.ParseName("System.Runtime.CompilerServices.CallerMemberName").WithAdditionalAnnotations(Simplifier.Annotation))));

        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(WPF1013UseCallerMemberName.DiagnosticId);

        /// <inheritdoc/>
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

                var parameter = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                                 .FirstAncestorOrSelf<ParameterSyntax>();
                if (parameter != null)
                {
                    var updated = parameter.AddAttributeLists(CallerMemberName)
                                           .WithDefault(SyntaxFactory.EqualsValueClause(SyntaxFactory.ParseExpression("null")));
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            "Use [CallerMemberName]",
                            cancellationToken => Task.FromResult(context.Document.WithSyntaxRoot(syntaxRoot.ReplaceNode(parameter, updated))),
                            this.GetType().FullName),
                        diagnostic);
                }

                var invocation = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                 .FirstAncestorOrSelf<InvocationExpressionSyntax>();

                if (invocation != null)
                {
                    var updated = invocation.RemoveNode(invocation.ArgumentList.Arguments[0], SyntaxRemoveOptions.AddElasticMarker);
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            "Use [CallerMemberName]",
                            cancellationToken => Task.FromResult(context.Document.WithSyntaxRoot(syntaxRoot.ReplaceNode(invocation, updated))),
                            this.GetType().FullName),
                        diagnostic);
                }
            }
        }
    }
}