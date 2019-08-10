namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseSetCurrentValueFix))]
    [Shared]
    internal class UseSetCurrentValueFix : DocumentEditorCodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.WPF0041SetMutableUsingSetCurrentValue.Id);

        protected override DocumentEditorFixAllProvider FixAllProvider() => null;

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNode(diagnostic, out InvocationExpressionSyntax setValue) &&
                    setValue.Expression is MemberAccessExpressionSyntax invokedMemberAccess &&
                    invokedMemberAccess.Name is IdentifierNameSyntax identifierName)
                {
                    context.RegisterCodeFix(
                        setValue.ToString().Replace("SetValue", "SetCurrentValue"),
                        (editor, _) => editor.ReplaceNode(
                            identifierName,
                            x => x.WithIdentifier(SyntaxFactory.Identifier("SetCurrentValue"))
                                  .WithTriviaFrom(x)),
                        this.GetType()
                            .FullName,
                        diagnostic);
                }
                else if (syntaxRoot.TryFindNode(diagnostic, out AssignmentExpressionSyntax assignment) &&
                         assignment.Left is MemberAccessExpressionSyntax assignedMemberAccess &&
                         diagnostic.Properties.TryGetValue(nameof(BackingFieldOrProperty), out var backingFieldOrProperty))
                {
                    var expressionString = $"{assignedMemberAccess.Expression}.SetCurrentValue({backingFieldOrProperty}, {assignment.Right})";
                    context.RegisterCodeFix(
                        expressionString,
                        (editor, _) => editor.ReplaceNode(
                            assignment,
                            x => SyntaxFactory.ParseExpression(expressionString)
                                              .WithSimplifiedNames()
                                              .WithTriviaFrom(x)),
                        this.GetType()
                            .FullName,
                        diagnostic);
                }
            }
        }
    }
}
