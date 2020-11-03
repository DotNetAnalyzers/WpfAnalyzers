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
    using Microsoft.CodeAnalysis.Editing;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AttachedPropertyBrowsableForTypeAttributeFix))]
    [Shared]
    internal class AttachedPropertyBrowsableForTypeAttributeFix : DocumentEditorCodeFixProvider
    {
        private static readonly AttributeSyntax Attribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName("System.Windows.AttachedPropertyBrowsableForTypeAttribute")).WithSimplifiedNames();

        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.WPF0071ConverterDoesNotHaveAttribute.Id,
            Descriptors.WPF0033UseAttachedPropertyBrowsableForTypeAttribute.Id);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                                   .ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot is { } &&
                    syntaxRoot.TryFindNodeOrAncestor(diagnostic, out MethodDeclarationSyntax? methodDeclaration) &&
                    methodDeclaration.ParameterList is { } parameterList &&
                    parameterList.Parameters.TrySingle(out var parameter))
                {
                    context.RegisterCodeFix(
                        $"Add [AttachedPropertyBrowsableForType(typeof({parameter.Type}))].",
                        (editor, _) => editor.AddAttribute(
                            methodDeclaration,
                            editor.Generator.AddAttributeArguments(
                                Attribute,
                                new[] { editor.Generator.AttributeArgument(editor.Generator.TypeOfExpression(parameter.Type)) })),
                        this.GetType().FullName,
                        diagnostic);
                }
            }
        }
    }
}
