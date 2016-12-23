namespace WpfAnalyzers
{
    using System.Collections.Generic;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class Names
    {
        internal static bool UsesUnderscoreNames(this SyntaxNode node)
        {
            var typeDeclarationSyntax = node.FirstAncestorOrSelf<TypeDeclarationSyntax>();
            if (typeDeclarationSyntax == null)
            {
                return false;
            }

            foreach (var member in typeDeclarationSyntax.Members)
            {
                var field = member as FieldDeclarationSyntax;
                if (field == null ||
                    field.Modifiers.Any(SyntaxKind.StaticKeyword) ||
                    !field.Modifiers.Any(SyntaxKind.PrivateKeyword))
                {
                    continue;
                }

                foreach (var variable in field.Declaration.Variables)
                {
                    return variable.Identifier.ValueText.StartsWith("_");
                }
            }

            using (var pooled = ThisExpressionWalker.Create(typeDeclarationSyntax))
            {
                return pooled.Item.ThisExpressions.Count > 0;
            }
        }

        internal sealed class ThisExpressionWalker : CSharpSyntaxWalker
        {
            private static readonly Pool<ThisExpressionWalker> Cache = new Pool<ThisExpressionWalker>(
                () => new ThisExpressionWalker(),
                x => x.thisExpressions.Clear());

            private readonly List<ThisExpressionSyntax> thisExpressions = new List<ThisExpressionSyntax>();

            private ThisExpressionWalker()
            {
            }

            public IReadOnlyList<ThisExpressionSyntax> ThisExpressions => this.thisExpressions;

            public static Pool<ThisExpressionWalker>.Pooled Create(SyntaxNode node)
            {
                var pooled = Cache.GetOrCreate();
                pooled.Item.Visit(node);
                return pooled;
            }

            public override void VisitThisExpression(ThisExpressionSyntax node)
            {
                this.thisExpressions.Add(node);
                base.VisitThisExpression(node);
            }
        }
    }
}
