namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RenameMethodCodeFixProvider))]
    [Shared]
    internal class RenameMethodCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(
                WPF0004ClrMethodShouldMatchRegisteredName.DiagnosticId,
                WPF0005PropertyChangedCallbackShouldMatchRegisteredName.DiagnosticId,
                WPF0006CoerceValueCallbackShouldMatchRegisteredName.DiagnosticId);

        /// <inheritdoc/>
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var syntaxRoot = await document.GetSyntaxRootAsync(context.CancellationToken)
                                           .ConfigureAwait(false);
            var semanticModel = await document.GetSemanticModelAsync(context.CancellationToken)
                                             .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                var token = syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start);
                if (string.IsNullOrEmpty(token.ValueText))
                {
                    continue;
                }

                if (diagnostic.Id == WPF0004ClrMethodShouldMatchRegisteredName.DiagnosticId)
                {
                    var methodDeclaration = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                                      .FirstAncestorOrSelf<MethodDeclarationSyntax>();

                    if (methodDeclaration == null || methodDeclaration.IsMissing)
                    {
                        continue;
                    }

                    IFieldSymbol backingField;
                    if (ClrMethod.IsAttachedSetMethod(
                        methodDeclaration,
                        semanticModel,
                        context.CancellationToken,
                        out backingField))
                    {
                        TryUpdateName(
                            context,
                            backingField,
                            semanticModel,
                            syntaxRoot,
                            token,
                            "Set",
                            diagnostic);

                        continue;
                    }

                    if (ClrMethod.IsAttachedGetMethod(
                        methodDeclaration,
                        semanticModel,
                        context.CancellationToken,
                        out backingField))
                    {
                        TryUpdateName(
                            context,
                            backingField,
                            semanticModel,
                            syntaxRoot,
                            token,
                            "Get",
                            diagnostic);
                    }
                }
                else if (diagnostic.Id == WPF0005PropertyChangedCallbackShouldMatchRegisteredName.DiagnosticId)
                {
                    var node = syntaxRoot.FindNode(diagnostic.Location.SourceSpan);
                    var callback = node.FirstAncestorOrSelf<ArgumentSyntax>();
                    IdentifierNameSyntax nameExpression;
                    string registeredName;
                    if (WPF0005PropertyChangedCallbackShouldMatchRegisteredName.TryGetIdentifierAndRegisteredName(
                        callback,
                        semanticModel,
                        context.CancellationToken,
                        out nameExpression,
                        out registeredName))
                    {
                        var newName = $"On{registeredName}Changed";
                        context.RegisterCodeFix(
                            CodeAction.Create(
                                $"Rename to: {newName}",
                                cancellationToken => RenameHelper.RenameSymbolAsync(context.Document, syntaxRoot, token, newName, cancellationToken),
                                nameof(RenameMethodCodeFixProvider)),
                            diagnostic);
                    }
                }
                else if (diagnostic.Id == WPF0006CoerceValueCallbackShouldMatchRegisteredName.DiagnosticId)
                {
                    var node = syntaxRoot.FindNode(diagnostic.Location.SourceSpan);
                    var callback = node.FirstAncestorOrSelf<ArgumentSyntax>();
                    IdentifierNameSyntax nameExpression;
                    string registeredName;
                    if (WPF0006CoerceValueCallbackShouldMatchRegisteredName.TryGetIdentifierAndRegisteredName(
                        callback,
                        semanticModel,
                        context.CancellationToken,
                        out nameExpression,
                        out registeredName))
                    {
                        var newName = $"Coerce{registeredName}";
                        context.RegisterCodeFix(
                            CodeAction.Create(
                                $"Rename to: {newName}",
                                cancellationToken => RenameHelper.RenameSymbolAsync(context.Document, syntaxRoot, token, newName, cancellationToken),
                                nameof(RenameMethodCodeFixProvider)),
                            diagnostic);
                    }
                }
            }
        }

        private static void TryUpdateName(
            CodeFixContext context,
            IFieldSymbol setField,
            SemanticModel semanticModel,
            SyntaxNode syntaxRoot,
            SyntaxToken token,
            string prefix,
            Diagnostic diagnostic)
        {
            string registeredName;
            if (DependencyProperty.TryGetRegisteredName(
                setField,
                semanticModel,
                context.CancellationToken,
                out registeredName))
            {
                var newName = $"{prefix}{registeredName}";
                context.RegisterCodeFix(
                    CodeAction.Create(
                        $"Rename to: {newName}",
                        cancellationToken => RenameHelper.RenameSymbolAsync(context.Document, syntaxRoot, token, newName, cancellationToken),
                        nameof(RenameMethodCodeFixProvider)),
                    diagnostic);
            }
        }
    }
}