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

        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(WPF0071ConverterDoesNotHaveAttribute.Descriptor.Id, WPF0033UseAttachedPropertyBrowsableForTypeAttribute.Descriptor.Id);

        /// <inheritdoc/>
        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var document = context.Document;
            var syntaxRoot = await document.GetSyntaxRootAsync(context.CancellationToken)
                                           .ConfigureAwait(false);
            var semanticModel = await document.GetSemanticModelAsync(context.CancellationToken)
                                              .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                var token = syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start);
                if (string.IsNullOrEmpty(token.ValueText))
                {
                    continue;
                }

                if (syntaxRoot.FindNode(diagnostic.Location.SourceSpan).TryFirstAncestorOrSelf<MethodDeclarationSyntax>(out var methodDeclaration) &&
                    semanticModel.TryGetSymbol(methodDeclaration, context.CancellationToken, out var method) &&
                    method.Parameters.TrySingle(out var parameter))
                {
                    context.RegisterCodeFix(
                        $"Add [AttachedPropertyBrowsableForTypeAttribute(typeof({parameter.Type})))].",
                        (e, _) => AddAttribute(e, methodDeclaration, parameter.Type),
                        this.GetType().FullName,
                        diagnostic);
                }
            }
        }

        private static void AddAttribute(DocumentEditor editor, MethodDeclarationSyntax classDeclaration, ITypeSymbol type)
        {
            TypeOfExpressionSyntax TypeOf(ITypeSymbol t)
            {
                if (t != null)
                {
                    return (TypeOfExpressionSyntax)editor.Generator.TypeOfExpression(editor.Generator.TypeExpression(t));
                }

                return (TypeOfExpressionSyntax)editor.Generator.TypeOfExpression(SyntaxFactory.ParseTypeName("TYPE"));
            }

            editor.AddAttribute(
                classDeclaration,
                editor.Generator.AddAttributeArguments(
                    Attribute,
                    new[] { editor.Generator.AttributeArgument(TypeOf(type)) }));
        }
    }
}
