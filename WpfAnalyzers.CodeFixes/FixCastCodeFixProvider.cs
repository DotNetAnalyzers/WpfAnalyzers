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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FixCastCodeFixProvider))]
    [Shared]
    internal class FixCastCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            WPF0019CastSenderToCorrectType.DiagnosticId,
            WPF0020CastValueToCorrectType.DiagnosticId,
            WPF0021DirectCastSenderToExactType.DiagnosticId,
            WPF0022DirectCastValueToExactType.DiagnosticId);

        public override FixAllProvider GetFixAllProvider() => DocumentEditorFixAllProvider.Default;

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

                if (diagnostic.Properties.TryGetValue("ExpectedType", out var registeredType))
                {
                    var node = syntaxRoot.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
                    if (node.FirstAncestorOrSelf<IdentifierNameSyntax>() is IdentifierNameSyntax identifierName &&
                        !identifierName.IsMissing)
                    {
                        context.RegisterDocumentEditorFix(
                            $"Change type to: {registeredType}.",
                            (e, _) => ChangeType(e, identifierName, registeredType),
                            this.GetType().FullName,
                            diagnostic);
                    }

                    if (node.FirstAncestorOrSelf<PredefinedTypeSyntax>() is PredefinedTypeSyntax predefinedType &&
                        !predefinedType.IsMissing)
                    {
                        context.RegisterDocumentEditorFix(
                            $"Change type to: {registeredType}.",
                            (e, _) => ChangeType(e, predefinedType, registeredType),
                            this.GetType().FullName,
                            diagnostic);
                    }

                    if (node.FirstAncestorOrSelf<QualifiedNameSyntax>() is QualifiedNameSyntax qualifiedName &&
                        !qualifiedName.IsMissing)
                    {
                        context.RegisterDocumentEditorFix(
                            $"Change type to: {registeredType}.",
                            (e, _) => e.ReplaceNode(
                                qualifiedName,
                                (x, __) => SyntaxFactory.ParseTypeName(registeredType)),
                            this.GetType().FullName,
                            diagnostic);
                    }
                }
            }
        }

        private static void ChangeType(DocumentEditor editor, PredefinedTypeSyntax typeSyntax, string newType)
        {
            editor.ReplaceNode(
                typeSyntax,
                (x, _) => SyntaxFactory.ParseTypeName(newType));
        }

        private static void ChangeType(DocumentEditor editor, IdentifierNameSyntax typeSyntax, string newType)
        {
            editor.ReplaceNode(
                typeSyntax,
                (x, _) => ((IdentifierNameSyntax)x).WithIdentifier(SyntaxFactory.ParseToken(newType)));
        }
    }
}