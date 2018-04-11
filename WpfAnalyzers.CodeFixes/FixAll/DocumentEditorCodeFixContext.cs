namespace WpfAnalyzers
{
    using System;
    using System.Collections.Immutable;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Editing;
    using Microsoft.CodeAnalysis.Text;

    public struct DocumentEditorCodeFixContext
    {
        private readonly CodeFixContext context;

        public DocumentEditorCodeFixContext(CodeFixContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Document corresponding to the <see cref="P:Microsoft.CodeAnalysis.CodeFixes.CodeFixContext.Span" /> to fix.
        /// </summary>
        public Document Document => this.context.Document;

        public CancellationToken CancellationToken => this.context.CancellationToken;

        /// <summary>
        /// Text span within the <see cref="P:Microsoft.CodeAnalysis.CodeFixes.CodeFixContext.Document" /> to fix.
        /// </summary>
        public TextSpan Span => this.context.Span;

        /// <summary>
        /// Diagnostics to fix.
        /// NOTE: All the diagnostics in this collection have the same <see cref="P:Microsoft.CodeAnalysis.CodeFixes.CodeFixContext.Span" />.
        /// </summary>
        public ImmutableArray<Diagnostic> Diagnostics => this.context.Diagnostics;

        public void RegisterCodeFix(
            string title,
            Action<DocumentEditor, CancellationToken> action,
            Type equivalenceKey,
            Diagnostic diagnostic)
        {
            this.context.RegisterCodeFix(
                new DocumentEditorAction(title, this.context.Document, action, equivalenceKey.FullName),
                diagnostic);
        }

        public void RegisterCodeFix(
            string title,
            Action<DocumentEditor, CancellationToken> action,
            Diagnostic diagnostic)
        {
            this.RegisterCodeFix(title, action, title, diagnostic);
        }

        public void RegisterCodeFix(
            string title,
            Action<DocumentEditor, CancellationToken> action,
            string equivalenceKey,
            Diagnostic diagnostic)
        {
            this.context.RegisterCodeFix(
                new DocumentEditorAction(title, this.context.Document, action, equivalenceKey),
                diagnostic);
        }
    }
}
