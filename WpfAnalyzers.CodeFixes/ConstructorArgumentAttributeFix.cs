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
        private static readonly AttributeSyntax Attribute = Simplify.WithSimplifiedNames(SyntaxFactory
                                     .Attribute(SyntaxFactory.ParseName("System.Windows.Markup.ConstructorArgumentAttribute")));

        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(WPF0083UseConstructorArgumentAttribute.DiagnosticId);

        /// <inheritdoc/>
        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
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
