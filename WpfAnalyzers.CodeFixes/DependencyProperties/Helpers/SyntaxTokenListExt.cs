namespace WpfAnalyzers.DependencyProperties
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    internal static class SyntaxTokenListExt
    {
        internal static SyntaxTokenList WithStatic(this SyntaxTokenList modifiers)
        {
            var index = 0;
            for (var i = 0; i < modifiers.Count; i++)
            {
                switch (modifiers[i].Kind())
                {
                    case SyntaxKind.StaticKeyword:
                        return modifiers;
                    case SyntaxKind.PublicKeyword:
                    case SyntaxKind.ProtectedKeyword:
                    case SyntaxKind.InternalKeyword:
                    case SyntaxKind.PrivateKeyword:
                        index = i + 1;
                        break;
                }
            }

            return modifiers.Insert(index, SyntaxFactory.Token(SyntaxKind.StaticKeyword));
        }

        internal static SyntaxTokenList WithReadOnly(this SyntaxTokenList modifiers)
        {
            var index = 0;
            for (var i = 0; i < modifiers.Count; i++)
            {
                switch (modifiers[i].Kind())
                {
                    case SyntaxKind.ReadOnlyKeyword:
                        return modifiers;
                    case SyntaxKind.StaticKeyword:
                    case SyntaxKind.PublicKeyword:
                    case SyntaxKind.ProtectedKeyword:
                    case SyntaxKind.InternalKeyword:
                    case SyntaxKind.PrivateKeyword:
                        index = i + 1;
                        break;
                }
            }

            return modifiers.Insert(index, SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword));
        }
    }
}