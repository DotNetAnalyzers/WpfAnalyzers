namespace WpfAnalyzers.DependencyProperties
{
    using System;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseDependencyPropertyKeyCodeFixProvider))]
    [Shared]
    internal class UseDependencyPropertyKeyCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(WA1220UseDependencyPropertyKeyForSettingReadOnlyProperties.DiagnosticId);

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

                SyntaxNode updated;
                if (TryFix(diagnostic, syntaxRoot, out updated))
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            "Use DependencyPropertyKey when setting a readonly property.",
                            _ => Task.FromResult(context.Document.WithSyntaxRoot(updated)),
                            nameof(MakeFieldStaticReadonlyCodeFixProvider)),
                        diagnostic);
                }
            }
        }

        private static bool TryFix(Diagnostic diagnostic, SyntaxNode syntaxRoot, out SyntaxNode result)
        {
            result = syntaxRoot;
            var invocation = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                             .FirstAncestorOrSelf<InvocationExpressionSyntax>();
            if (invocation == null || invocation.IsMissing)
            {
                return false;
            }

            SyntaxNode updated = invocation;
            result = syntaxRoot.ReplaceNode(invocation, updated);
            return true;
        }
    }
}