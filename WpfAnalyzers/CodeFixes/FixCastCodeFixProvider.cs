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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FixCastCodeFixProvider))]
    [Shared]
    internal class FixCastCodeFixProvider : DocumentEditorCodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            WPF0019CastSenderToCorrectType.Descriptor.Id,
            WPF0020CastValueToCorrectType.Descriptor.Id,
            WPF0021DirectCastSenderToExactType.Descriptor.Id,
            WPF0022DirectCastValueToExactType.Descriptor.Id);

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

                if (diagnostic.Properties.TryGetValue("ExpectedType", out var registeredType))
                {
                    var node = syntaxRoot.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
                    if (node.FirstAncestorOrSelf<IdentifierNameSyntax>() is IdentifierNameSyntax identifierName &&
                        !identifierName.IsMissing)
                    {
                        context.RegisterCodeFix(
                            $"Change type to: {registeredType}.",
                            (e, _) => ChangeType(e, identifierName, registeredType),
                            this.GetType().FullName,
                            diagnostic);
                    }

                    if (node.FirstAncestorOrSelf<PredefinedTypeSyntax>() is PredefinedTypeSyntax predefinedType &&
                        !predefinedType.IsMissing)
                    {
                        context.RegisterCodeFix(
                            $"Change type to: {registeredType}.",
                            (e, _) => ChangeType(e, predefinedType, registeredType),
                            this.GetType().FullName,
                            diagnostic);
                    }

                    if (node.FirstAncestorOrSelf<QualifiedNameSyntax>() is QualifiedNameSyntax qualifiedName &&
                        !qualifiedName.IsMissing)
                    {
                        context.RegisterCodeFix(
                            $"Change type to: {registeredType}.",
                            (e, _) => e.ReplaceNode(
                                qualifiedName,
#pragma warning disable SA1313 // Parameter names must begin with lower-case letter
                                (x, __) => SyntaxFactory.ParseTypeName(registeredType)),
#pragma warning restore SA1313 // Parameter names must begin with lower-case letter
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
