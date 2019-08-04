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
    using Microsoft.CodeAnalysis.Editing;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ConstructorArgumentAttributeFix))]
    [Shared]
    internal class ConstructorArgumentAttributeFix : DocumentEditorCodeFixProvider
    {
        private static readonly AttributeSyntax Attribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName("System.Windows.Markup.ConstructorArgumentAttribute")).WithSimplifiedNames();

        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(WPF0083UseConstructorArgumentAttribute.Descriptor.Id);

        /// <inheritdoc/>
        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var document = context.Document;
            var syntaxRoot = await document.GetSyntaxRootAsync(context.CancellationToken)
                                           .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNodeOrAncestor<PropertyDeclarationSyntax>(diagnostic, out var propertyDeclaration) &&
                    diagnostic.Properties.TryGetValue(nameof(ConstructorArgument), out var parameterName))
                {
                    context.RegisterCodeFix(
                        "Add ConstructorArgumentAttribute.",
                        (e, _) => AddAttribute(e, propertyDeclaration, parameterName),
                        "Add ConstructorArgumentAttribute.",
                        diagnostic);
                }
            }
        }

        private static void AddAttribute(DocumentEditor editor, PropertyDeclarationSyntax propertyDeclaration, string parameterName)
        {
            editor.AddAttribute(
                propertyDeclaration,
                editor.Generator.AddAttributeArguments(
                    Attribute,
                    new[] { editor.Generator.AttributeArgument(editor.Generator.LiteralExpression(parameterName)) }));
        }
    }
}
