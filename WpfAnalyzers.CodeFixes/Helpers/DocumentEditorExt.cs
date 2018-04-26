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
        internal static DocumentEditor MakeSealed(this DocumentEditor editor, ClassDeclarationSyntax containingType)
        {
            editor.ReplaceNode(containingType, (node, generator) => MakeSealedRewriter.Default.Visit(node, (ClassDeclarationSyntax)node));
            return editor;
        }

        private class MakeSealedRewriter : CSharpSyntaxRewriter
        {
            public static readonly MakeSealedRewriter Default = new MakeSealedRewriter();

            private static readonly ThreadLocal<ClassDeclarationSyntax> CurrentClass = new ThreadLocal<ClassDeclarationSyntax>();

            public SyntaxNode Visit(SyntaxNode node, ClassDeclarationSyntax classDeclaration)
            {
                CurrentClass.Value = classDeclaration;
                var updated = this.Visit(node);
                CurrentClass.Value = null;
                return updated;
            }

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
                if (node.Modifiers.TrySingle(x => x.IsKind(SyntaxKind.VirtualKeyword), out SyntaxToken modifier))
                {
                    node = node.WithModifiers(node.Modifiers.Remove(modifier));
                }

                if (node.Modifiers.TrySingle(x => x.IsKind(SyntaxKind.ProtectedKeyword), out modifier))
                {
                    node = node.WithModifiers(node.Modifiers.Replace(modifier, SyntaxFactory.Token(SyntaxKind.PrivateKeyword)));
                }

                return base.VisitFieldDeclaration(node);
            }

            public override SyntaxNode VisitEventDeclaration(EventDeclarationSyntax node)
            {
                if (node.Modifiers.TrySingle(x => x.IsKind(SyntaxKind.VirtualKeyword), out SyntaxToken modifier))
                {
                    node = node.WithModifiers(node.Modifiers.Remove(modifier));
                }

                if (node.Modifiers.TrySingle(x => x.IsKind(SyntaxKind.ProtectedKeyword), out modifier) &&
                    !node.Modifiers.Any(SyntaxKind.OverrideKeyword))
                {
                    node = node.WithModifiers(node.Modifiers.Replace(modifier, SyntaxFactory.Token(SyntaxKind.PrivateKeyword)));
                }

                return base.VisitEventDeclaration(node);
            }

            public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
            {
                if (node.Modifiers.TrySingle(x => x.IsKind(SyntaxKind.VirtualKeyword), out SyntaxToken modifier))
                {
                    node = node.WithModifiers(node.Modifiers.Remove(modifier));
                }

                if (node.Modifiers.TrySingle(x => x.IsKind(SyntaxKind.ProtectedKeyword), out modifier) &&
                    !node.Modifiers.Any(SyntaxKind.OverrideKeyword))
                {
                    node = node.WithModifiers(node.Modifiers.Replace(modifier, SyntaxFactory.Token(SyntaxKind.PrivateKeyword)));
                }

                return base.VisitPropertyDeclaration(node);
            }

            public override SyntaxNode VisitAccessorDeclaration(AccessorDeclarationSyntax node)
            {
                if (node.Modifiers.TrySingle(x => x.IsKind(SyntaxKind.VirtualKeyword), out SyntaxToken modifier))
                {
                    node = node.WithModifiers(node.Modifiers.Remove(modifier));
                }

                var parentModifiers = node.FirstAncestor<BasePropertyDeclarationSyntax>()?.Modifiers;
                if (node.Modifiers.TrySingle(x => x.IsKind(SyntaxKind.ProtectedKeyword), out modifier) &&
                    parentModifiers?.Any(SyntaxKind.OverrideKeyword) == false)
                {
                    node = node.WithModifiers(node.Modifiers.Replace(modifier, SyntaxFactory.Token(SyntaxKind.PrivateKeyword)));
                }

                if (parentModifiers?.TrySingle(x => x.IsKind(SyntaxKind.PrivateKeyword), out modifier) == true)
                {
                    if (node.Modifiers.TrySingle(x => x.IsKind(SyntaxKind.PrivateKeyword), out modifier))
                    {
                        node = node.WithModifiers(node.Modifiers.Remove(modifier));
                    }
                }

                return base.VisitAccessorDeclaration(node);
            }

            public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                if (node.Modifiers.TrySingle(x => x.IsKind(SyntaxKind.VirtualKeyword), out SyntaxToken modifier))
                {
                    node = node.WithModifiers(node.Modifiers.Remove(modifier));
                }

                if (node.Modifiers.TrySingle(x => x.IsKind(SyntaxKind.ProtectedKeyword), out modifier) &&
                    !node.Modifiers.Any(SyntaxKind.OverrideKeyword))
                {
                    node = node.WithModifiers(node.Modifiers.Replace(modifier, SyntaxFactory.Token(SyntaxKind.PrivateKeyword)));
                }

                return base.VisitMethodDeclaration(node);
            }
        }
    }
}
