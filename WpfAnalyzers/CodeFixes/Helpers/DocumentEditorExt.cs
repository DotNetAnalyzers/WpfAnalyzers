namespace WpfAnalyzers
{
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;

    internal static class DocumentEditorExt
    {
        internal static DocumentEditor MakeSealed(this DocumentEditor editor, ClassDeclarationSyntax classDeclaration)
        {
            if (classDeclaration.Modifiers.Any(SyntaxKind.SealedKeyword))
            {
                return editor;
            }

            editor.ReplaceNode(classDeclaration, (node, generator) => MakeSealedRewriter.Default.Visit(node, (ClassDeclarationSyntax)node));
            return editor;
        }

        private class MakeSealedRewriter : CSharpSyntaxRewriter
        {
            internal static readonly MakeSealedRewriter Default = new MakeSealedRewriter();

            private static readonly ThreadLocal<ClassDeclarationSyntax> CurrentClass = new ThreadLocal<ClassDeclarationSyntax>();

            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                // We only want to make the top level class sealed.
                if (ReferenceEquals(CurrentClass.Value, node))
                {
                    var updated = (ClassDeclarationSyntax)base.VisitClassDeclaration(node);
                    return updated.WithModifiers(updated.Modifiers.Add(SyntaxFactory.Token(SyntaxKind.SealedKeyword)));
                }

                return node;
            }

            public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
            {
                if (TryUpdate(node.Modifiers, out var modifiers))
                {
                    return node.WithModifiers(modifiers);
                }

                return node;
            }

            public override SyntaxNode VisitEventDeclaration(EventDeclarationSyntax node)
            {
                if (TryUpdate(node.Modifiers, out var modifiers))
                {
                    node = node.WithModifiers(modifiers);
                }

                return base.VisitEventDeclaration(node);
            }

            public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
            {
                if (TryUpdate(node.Modifiers, out var modifiers))
                {
                    node = node.WithModifiers(modifiers);
                }

                return base.VisitPropertyDeclaration(node);
            }

            public override SyntaxNode VisitAccessorDeclaration(AccessorDeclarationSyntax node)
            {
                if (node.TryFirstAncestor(out BasePropertyDeclarationSyntax parent) &&
                    parent.Modifiers.Any(SyntaxKind.PrivateKeyword) &&
                    node.Modifiers.TrySingle(x => x.IsKind(SyntaxKind.PrivateKeyword), out var modifier))
                {
                    return node.WithModifiers(node.Modifiers.Remove(modifier));
                }

                return TryUpdate(node.Modifiers, out var modifiers)
                    ? node.WithModifiers(modifiers)
                    : node;
            }

            public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                if (TryUpdate(node.Modifiers, out var modifiers))
                {
                    return node.WithModifiers(modifiers);
                }

                return node;
            }

            internal SyntaxNode Visit(SyntaxNode node, ClassDeclarationSyntax classDeclaration)
            {
                CurrentClass.Value = classDeclaration;
                var updated = this.Visit(node);
                CurrentClass.Value = null;
                return updated;
            }

            private static bool TryUpdate(SyntaxTokenList modifiers, out SyntaxTokenList result)
            {
                result = modifiers;
                if (modifiers.TrySingle(x => x.IsKind(SyntaxKind.VirtualKeyword), out var modifier))
                {
                    result = modifiers.Remove(modifier);
                }

                if (result.TrySingle(x => x.IsKind(SyntaxKind.ProtectedKeyword), out modifier))
                {
                    result = result.Replace(modifier, SyntaxFactory.Token(SyntaxKind.PrivateKeyword));
                }

                return result != modifiers;
            }
        }
    }
}
