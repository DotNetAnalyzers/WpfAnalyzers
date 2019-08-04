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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DocumentClrMethodFix))]
    [Shared]
    internal class DocumentClrMethodFix : DocumentEditorCodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            WPF0061DocumentClrMethod.Descriptor.Id);

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
                                                    .AppendLine($"/// <summary>Helper for getting <see cref=\"{fieldOrProperty.Name}\"/> from <paramref name=\"{parameter.Name}\"/>.</summary>")
                                                    .AppendLine($"/// <param name=\"{parameter.Name}\"><see cref=\"{parameter.Type.ToMinimalDisplayString(semanticModel, methodDeclaration.SpanStart, SymbolDisplayFormat.MinimallyQualifiedFormat)}\"/> to read <see cref=\"{fieldOrProperty.Name}\"/> from.</param>")
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
                                                    .AppendLine($"/// <summary>Helper for setting <see cref=\"{fieldOrProperty.Name}\"/> on <paramref name=\"{parameter.Name}\"/>.</summary>")
                                                    .AppendLine($"/// <param name=\"{parameter.Name}\"><see cref=\"{parameter.Type.ToMinimalDisplayString(semanticModel, methodDeclaration.SpanStart, SymbolDisplayFormat.MinimallyQualifiedFormat)}\"/> to set <see cref=\"{fieldOrProperty.Name}\"/> on.</param>")
                                                    .AppendLine($"/// <param name=\"{method.Parameters[1].Name}\">{registeredName} property value.</param>")
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
