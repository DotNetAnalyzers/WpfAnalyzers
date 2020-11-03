namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ConstructorArgumentAttributeArgumentFix))]
    [Shared]
    internal class ConstructorArgumentAttributeArgumentFix : DocumentEditorCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.WPF0082ConstructorArgument.Id);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var document = context.Document;
            var syntaxRoot = await document.GetSyntaxRootAsync(context.CancellationToken)
                                           .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot is { } &&
                    syntaxRoot.TryFindNodeOrAncestor<AttributeArgumentSyntax>(diagnostic, out var argument) &&
                    argument is { Expression: { } expression } &&
                    diagnostic.Properties.TryGetValue(nameof(ConstructorArgument), out var parameterName))
                {
                    context.RegisterCodeFix(
                        $"[ConstructorArgument(\"{parameterName})\"))].",
                        (e, _) => e.ReplaceNode(
                            expression,
                            x => e.Generator.LiteralExpression(parameterName).WithTriviaFrom(x)),
                        this.GetType(),
                        diagnostic);
                }
            }
        }
    }
}
