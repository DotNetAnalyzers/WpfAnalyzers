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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ConstructorArgumentAttributeFix))]
    [Shared]
    internal class ConstructorArgumentAttributeFix : CodeFixProvider
    {
        private static readonly AttributeSyntax Attribute = SyntaxFactory
            .Attribute(SyntaxFactory.ParseName("System.Windows.Markup.ConstructorArgumentAttribute"))
            .WithSimplifiedNames();

        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(WPF0083UseConstructorArgumentAttribute.DiagnosticId);

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        /// <inheritdoc/>
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var syntaxRoot = await document.GetSyntaxRootAsync(context.CancellationToken)
                                           .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                var token = syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start);
                if (string.IsNullOrEmpty(token.ValueText))
                {
                    continue;
                }

                var propertyDeclaration = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                                 .FirstAncestorOrSelf<PropertyDeclarationSyntax>();
                if (propertyDeclaration != null &&
                    ConstructorArgument.IsAssigned(propertyDeclaration, out var parameterName))
                {
                    context.RegisterDocumentEditorFix(
                        $"Add ConstructorArgumentAttribute.",
                        (e, _) => AddAttribute(e, propertyDeclaration, parameterName),
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