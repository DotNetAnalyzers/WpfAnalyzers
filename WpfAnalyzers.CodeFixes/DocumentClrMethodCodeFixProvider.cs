namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;
    using Microsoft.CodeAnalysis.Formatting;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DocumentClrMethodCodeFixProvider))]
    [Shared]
    internal class DocumentClrMethodCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(WPF0061ClrMethodShouldHaveDocs.DiagnosticId);

        /// <inheritdoc/>
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

                var member = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                       .FirstAncestorOrSelf<MethodDeclarationSyntax>();
                if (member != null)
                {
                    context.RegisterDocumentEditorFix(
                        "Add xml documentation.",
                        (editor, cancellationToken) => AddDocumentation(editor, member, cancellationToken),
                        this.GetType(),
                        diagnostic);
                }
            }
        }

        private static void AddDocumentation(DocumentEditor editor, MethodDeclarationSyntax methodDeclaration, CancellationToken cancellationToken)
        {
            var method = editor.SemanticModel.GetDeclaredSymbolSafe(methodDeclaration, cancellationToken);
            if (ClrMethod.IsAttachedGetMethod(method, editor.SemanticModel, cancellationToken, out var fieldOrProperty) &&
                DependencyProperty.TryGetRegisteredName(fieldOrProperty, editor.SemanticModel, cancellationToken, out var registeredName))
            {
                editor.ReplaceNode(
                    methodDeclaration,
                    methodDeclaration.WithLeadingTrivia(
                        methodDeclaration.GetLeadingTrivia()
                                         .AddRange(SyntaxFactory.ParseLeadingTrivia(GetMethodDocs(method.Parameters[0].Type, registeredName))))
                                         .WithAdditionalAnnotations(Formatter.Annotation));
            }
            else if (ClrMethod.IsAttachedSetMethod(method, editor.SemanticModel, cancellationToken, out fieldOrProperty) &&
                     DependencyProperty.TryGetRegisteredName(fieldOrProperty, editor.SemanticModel, cancellationToken, out registeredName))
            {
                editor.ReplaceNode(
                    methodDeclaration,
                    methodDeclaration.WithLeadingTrivia(
                        methodDeclaration.GetLeadingTrivia()
                                         .AddRange(SyntaxFactory.ParseLeadingTrivia(SetMethodDocs(method.Parameters[0].Type, registeredName))))
                                         .WithAdditionalAnnotations(Formatter.Annotation));
            }
        }

        private static string GetMethodDocs(ITypeSymbol type, string registeredName)
        {
            return StringBuilderPool.Borrow()
                                    .AppendLine("/// <summary>")
                                    .AppendLine($"/// Helper for reading {registeredName} property from a {type.Name}.")
                                    .AppendLine("/// </summary>")
                                    .AppendLine($"/// <param name=\"element\">{type.Name} to read {registeredName} property from.</param>")
                                    .AppendLine($"/// <returns>{registeredName} property value.</returns>")
                                    .Return();
        }

        private static string SetMethodDocs(ITypeSymbol type, string registeredName)
        {
            return StringBuilderPool.Borrow()
                                    .AppendLine("/// <summary>")
                                    .AppendLine($"/// Helper for setting {registeredName} property on a {type.Name}.")
                                    .AppendLine("/// </summary>")
                                    .AppendLine($"/// <param name=\"element\">{type.Name} to set {registeredName} property on.</param>")
                                    .AppendLine($"/// <param name=\"value\">{registeredName} property value.</param>")
                                    .Return();
        }
    }
}