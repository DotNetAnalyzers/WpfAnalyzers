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
    using WpfAnalyzers.PropertyChanged.Helpers;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseCallerMemberNameCodeFixProvider))]
    [Shared]
    internal class UseCallerMemberNameCodeFixProvider : CodeFixProvider
    {
        private static readonly AttributeListSyntax CallerMemberName =
            SyntaxFactory.AttributeList(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Attribute(
                        SyntaxFactory.ParseName("System.Runtime.CompilerServices.CallerMemberName")
                                     .WithAdditionalAnnotations(Simplifier.Annotation))));

        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(WPF1013UseCallerMemberName.DiagnosticId);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

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

                var parameter = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                          .FirstAncestorOrSelf<ParameterSyntax>();
                if (parameter != null)
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            "Use [CallerMemberName]",
                            cancellationToken =>
                                Task.FromResult(
                                    context.Document.WithSyntaxRoot(
                                        syntaxRoot.ReplaceNode(parameter, AsCallerMemberName(parameter)))),
                            this.GetType().FullName),
                        diagnostic);
                    continue;
                }

                var invocation = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                           .FirstAncestorOrSelf<InvocationExpressionSyntax>();
                if (invocation != null)
                {
                    if (semanticModel.GetSymbolSafe(invocation, context.CancellationToken) is IMethodSymbol method &&
                        method.Parameters.Length == 1 &&
                        !method.Parameters[0].IsCallerMemberName())
                    {
                        foreach (var declaration in method.Declarations(context.CancellationToken))
                        {
                            var methodDeclaration = declaration as MethodDeclarationSyntax;
                            if (methodDeclaration == null)
                            {
                                continue;
                            }

                            var nameParameter = methodDeclaration.ParameterList.Parameters[0];

                            context.RegisterCodeFix(
                                CodeAction.Create(
                                    "Use [CallerMemberName]",
                                    cancellationToken =>
                                        Task.FromResult(
                                            context.Document.WithSyntaxRoot(
                                                syntaxRoot.ReplaceNode(
                                                    nameParameter,
                                                    AsCallerMemberName(nameParameter)))),
                                    this.GetType().FullName),
                                diagnostic);
                        }
                    }

                    var updated = invocation.RemoveNode(
                        invocation.ArgumentList.Arguments[0],
                        SyntaxRemoveOptions.AddElasticMarker);
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            "Use [CallerMemberName]",
                            cancellationToken =>
                                Task.FromResult(
                                    context.Document.WithSyntaxRoot(syntaxRoot.ReplaceNode(invocation, updated))),
                            this.GetType().FullName),
                        diagnostic);
                }
            }
        }

        private static ParameterSyntax AsCallerMemberName(ParameterSyntax parameter)
        {
            return parameter.AddAttributeLists(CallerMemberName)
                            .WithDefault(SyntaxFactory.EqualsValueClause(SyntaxFactory.ParseExpression("null")));
        }
    }
}