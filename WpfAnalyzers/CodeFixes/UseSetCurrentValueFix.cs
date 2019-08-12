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

            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                       .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNodeOrAncestor(diagnostic, out InvocationExpressionSyntax setValue) &&
                    TryGetName(setValue, out var identifierName))
                {
                    context.RegisterCodeFix(
                        setValue.ToString().Replace("SetValue", "SetCurrentValue"),
                        (editor, _) => editor.ReplaceNode(
                            identifierName,
                            x => x.WithIdentifier(SyntaxFactory.Identifier("SetCurrentValue")).WithTriviaFrom(x)),
                        nameof(UseSetCurrentValueFix),
                        diagnostic);
                }
                else if (syntaxRoot.TryFindNodeOrAncestor(diagnostic, out AssignmentExpressionSyntax assignment) &&
                         TryCreatePath(assignment, out var path) &&
                         diagnostic.Properties.TryGetValue(nameof(BackingFieldOrProperty), out var backingFieldOrProperty))
                {
                    var expressionString = $"{path}SetCurrentValue({backingFieldOrProperty}, {Cast(assignment)}{assignment.Right})";
                    context.RegisterCodeFix(
                        expressionString,
                        (editor, _) => editor.ReplaceNode(
                            assignment,
                            x => SyntaxFactory.ParseExpression(expressionString).WithTriviaFrom(x)),
                        nameof(UseSetCurrentValueFix),
                        diagnostic);
                }

                bool TryGetName(InvocationExpressionSyntax invocation, out SimpleNameSyntax result)
                {
                    switch (invocation.Expression)
                    {
                        case MemberAccessExpressionSyntax memberAccess:
                            result = memberAccess.Name;
                            return true;
                        case IdentifierNameSyntax name:
                            result = name;
                            return true;
                        case MemberBindingExpressionSyntax memberBinding:
                            result = memberBinding.Name;
                            return true;
                        default:
                            result = null;
                            return true;
                    }
                }

                bool TryCreatePath(AssignmentExpressionSyntax assignment, out string result)
                {
                    switch (assignment.Left)
                    {
                        case MemberAccessExpressionSyntax memberAccess:
                            result = $"{memberAccess.Expression}.";
                            return true;
                        case IdentifierNameSyntax _:
                            result = string.Empty;
                            return true;
                        default:
                            result = null;
                            return false;
                    }
                }

                string Cast(AssignmentExpressionSyntax assignment)
                {
                    if (semanticModel.TryGetType(assignment.Left, context.CancellationToken, out var type) &&
                        type != KnownSymbols.Object &&
                        !assignment.Right.IsRepresentationPreservingConversion(type, semanticModel))
                    {
                        return $"({type.ToMinimalDisplayString(semanticModel, assignment.SpanStart)})";
                    }

                    return string.Empty;
                }
            }
        }
    }
}
