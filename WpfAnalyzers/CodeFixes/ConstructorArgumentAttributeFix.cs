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
            ImmutableArray.Create(Descriptors.WPF0083UseConstructorArgumentAttribute.Id);

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
                        (e, _) => e.AddAttribute(
                            propertyDeclaration,
                            e.Generator.AddAttributeArguments(
                                Attribute,
                                new[] { e.Generator.AttributeArgument(e.Generator.LiteralExpression(parameterName)) })),
                        "Add ConstructorArgumentAttribute.",
                        diagnostic);
                }
            }
        }
    }
}
