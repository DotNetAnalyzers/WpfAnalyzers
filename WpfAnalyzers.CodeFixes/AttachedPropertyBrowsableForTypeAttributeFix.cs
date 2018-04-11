namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AttachedPropertyBrowsableForTypeAttributeFix))]
    [Shared]
    internal class AttachedPropertyBrowsableForTypeAttributeFix : DocumentEditorCodeFixProvider
    {
        private static readonly AttributeSyntax Attribute = SyntaxFactory
            .Attribute(SyntaxFactory.ParseName("System.Windows.AttachedPropertyBrowsableForTypeAttribute"))
            .WithSimplifiedNames();

        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(WPF0071ConverterDoesNotHaveAttribute.DiagnosticId, WPF0033UseAttachedPropertyBrowsableForTypeAttribute.DiagnosticId);

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

                var methodDeclaration = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                                 .FirstAncestorOrSelf<MethodDeclarationSyntax>();
                if (AttachedPropertyBrowsableForType.TryGetParameterType(methodDeclaration, semanticModel, context.CancellationToken, out var type))
                {
                    context.RegisterCodeFix(
                        $"Add [AttachedPropertyBrowsableForTypeAttribute(typeof({type})))].",
                        (e, _) => AddAttribute(e, methodDeclaration, type),
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
