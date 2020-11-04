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
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.WPF0041SetMutableUsingSetCurrentValue.Id);

        protected override DocumentEditorFixAllProvider? FixAllProvider() => null;

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);

            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                       .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot is { } &&
                    semanticModel is { } &&
                    syntaxRoot.TryFindNodeOrAncestor(diagnostic, out AssignmentExpressionSyntax? assignment) &&
                    TryCreatePath(assignment) is { } path &&
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
                else if (syntaxRoot is { } &&
                         syntaxRoot.TryFindNodeOrAncestor(diagnostic, out InvocationExpressionSyntax? setValue) &&
                         Name(setValue) is { Identifier: { ValueText: "SetValue" } } name)
                {
                    context.RegisterCodeFix(
                        setValue.ToString().Replace("SetValue", "SetCurrentValue"),
                        (editor, _) => editor.ReplaceNode(
                            name,
                            x => x.WithIdentifier(SyntaxFactory.Identifier("SetCurrentValue")).WithTriviaFrom(x)),
                        nameof(UseSetCurrentValueFix),
                        diagnostic);
                }

                static SimpleNameSyntax? Name(InvocationExpressionSyntax invocation)
                {
                    return invocation.Expression switch
                    {
                        MemberAccessExpressionSyntax memberAccess => memberAccess.Name,
                        IdentifierNameSyntax name => name,
                        MemberBindingExpressionSyntax memberBinding => memberBinding.Name,
                        _ => null,
                    };
                }

                static string? TryCreatePath(AssignmentExpressionSyntax assignment)
                {
                    return assignment.Left switch
                    {
                        MemberAccessExpressionSyntax memberAccess => $"{memberAccess.Expression}.",
                        IdentifierNameSyntax _ => string.Empty,
                        _ => null,
                    };
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
