namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DocumentClrMethodCodeFixProvider))]
    [Shared]
    internal class DocumentClrMethodCodeFixProvider : DocumentEditorCodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            WPF0061DocumentClrMethod.DiagnosticId);

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
                if (syntaxRoot.TryFindNodeOrAncestor(diagnostic, out MethodDeclarationSyntax methodDeclaration) &&
                    semanticModel.TryGetSymbol(methodDeclaration, context.CancellationToken, out var method))
                {
                    if (ClrMethod.IsAttachedGet(method, semanticModel, context.CancellationToken, out var fieldOrProperty) &&
                        DependencyProperty.TryGetRegisteredName(fieldOrProperty, semanticModel, context.CancellationToken, out var registeredName))
                    {
                        var parameter = method.Parameters[0];
                        var text = StringBuilderPool.Borrow()
                                                    .AppendLine($"/// <summary>Helper for reading {registeredName} property from <paramref name=\"{parameter.Name}\"/>.</summary>")
                                                    .AppendLine($"/// <param name=\"element\">{parameter.Type.ToMinimalDisplayString(semanticModel, methodDeclaration.SpanStart, SymbolDisplayFormat.MinimallyQualifiedFormat)} to read {registeredName} property from.</param>")
                                                    .AppendLine($"/// <returns>{registeredName} property value.</returns>")
                                                    .Return();
                        context.RegisterCodeFix(
                            "Add standard documentation.",
                            (editor, _) => editor.ReplaceNode(methodDeclaration, x => x.WithDocumentationText(text)),
                            this.GetType(),
                            diagnostic);
                    }
                    else if (ClrMethod.IsAttachedSet(method, semanticModel, context.CancellationToken, out fieldOrProperty) &&
                             DependencyProperty.TryGetRegisteredName(fieldOrProperty, semanticModel, context.CancellationToken, out registeredName))
                    {
                        var parameter = method.Parameters[0];
                        var text = StringBuilderPool.Borrow()
                                                    .AppendLine($"/// <summary>Helper for setting {registeredName} property on <paramref name=\"{parameter.Name}\"/>.</summary>")
                                                    .AppendLine($"/// <param name=\"element\">{parameter.Type.ToMinimalDisplayString(semanticModel, methodDeclaration.SpanStart, SymbolDisplayFormat.MinimallyQualifiedFormat)} to set {registeredName} property on.</param>")
                                                    .AppendLine($"/// <param name=\"value\">{registeredName} property value.</param>")
                                                    .Return();
                        context.RegisterCodeFix(
                            "Add standard documentation.",
                            (editor, _) => editor.ReplaceNode(methodDeclaration, x => x.WithDocumentationText(text)),
                            this.GetType(),
                            diagnostic);
                    }
                }
            }
        }
    }
}
