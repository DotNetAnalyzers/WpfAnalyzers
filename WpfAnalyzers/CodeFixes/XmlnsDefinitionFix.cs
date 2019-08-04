namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Threading.Tasks;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Formatting;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(XmlnsDefinitionFix))]
    [Shared]
    internal class XmlnsDefinitionFix : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(Descriptors.WPF0052XmlnsDefinitionsDoesNotMapAllNamespaces.Id);

        public override FixAllProvider GetFixAllProvider() => null;

        /// <inheritdoc/>
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                              .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                var token = syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start);
                if (string.IsNullOrEmpty(token.ValueText))
                {
                    continue;
                }

                var attribute = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                     .FirstAncestorOrSelf<AttributeSyntax>();
                if (attribute == null || attribute.IsMissing)
                {
                    continue;
                }

                var toAdd = diagnostic.Properties.Values.Select(x => CreateAttribute(x, attribute))
                                      .Where(a => a != null)
                                      .ToArray();
                if (toAdd.Any())
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            "Map missing namespaces.",
                            _ => Task.FromResult(
                                context.Document.WithSyntaxRoot(
                                    syntaxRoot.InsertNodesAfter(
                                        attribute.FirstAncestorOrSelf<AttributeListSyntax>(),
                                        toAdd))),
                            this.GetType().FullName),
                        diagnostic);
                }
            }
        }

        private static AttributeListSyntax CreateAttribute(string @namespace, AttributeSyntax attribute)
        {
            var list = attribute.FirstAncestorOrSelf<AttributeListSyntax>();
            if (list?.Attributes.Count != 1)
            {
                return null;
            }

            if (!Attribute.TryFindArgument(attribute, 1, KnownSymbol.XmlnsDefinitionAttribute.ClrNamespaceArgumentName, out AttributeArgumentSyntax oldArgument))
            {
                return null;
            }

            var newArgument = oldArgument.WithExpression(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(@namespace)));
            return list.WithAttributes(SyntaxFactory.SingletonSeparatedList(attribute.ReplaceNode(oldArgument, newArgument))).WithAdditionalAnnotations(Formatter.Annotation);
        }
    }
}
