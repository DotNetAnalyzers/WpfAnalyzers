namespace WpfAnalyzers
{
    using System;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Editing;

    internal static class CodeFixContextExt
    {
        internal static void RegisterDocumentEditorFix(
            this CodeFixContext context,
            string title,
            Action<DocumentEditor, CancellationToken> action,
            Type equivalenceKey,
            Diagnostic diagnostic)
        {
            context.RegisterCodeFix(
                new DocumentEditorAction(title, context.Document, action, equivalenceKey.FullName),
                diagnostic);
        }

        internal static void RegisterDocumentEditorFix(
            this CodeFixContext context,
            string title,
            Action<DocumentEditor, CancellationToken> action,
            Diagnostic diagnostic)
        {
            RegisterDocumentEditorFix(context, title, action, title, diagnostic);
        }

        internal static void RegisterDocumentEditorFix(
            this CodeFixContext context,
            string title,
            Action<DocumentEditor, CancellationToken> action,
            string equivalenceKey,
            Diagnostic diagnostic)
        {
            context.RegisterCodeFix(
                new DocumentEditorAction(title, context.Document, action, equivalenceKey),
                diagnostic);
        }
    }
}