namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
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

            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNodeOrAncestor(diagnostic, out InvocationExpressionSyntax? invocation) &&
                    IdentifierName() is { } identifier)
                {
                    context.RegisterCodeFix(
                        "Use SetValue",
                        e => e.ReplaceNode(
                            identifier,
                            x => x.WithIdentifier(SyntaxFactory.Identifier("SetValue"))),
                        "Use SetValue",
                        diagnostic);
                }

                IdentifierNameSyntax? IdentifierName()
                {
                    return invocation!.Expression switch
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
