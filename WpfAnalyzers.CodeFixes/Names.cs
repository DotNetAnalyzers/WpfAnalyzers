namespace WpfAnalyzers
{
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

            using (var pooled = UsesThisWalker.Create(typeDeclarationSyntax))
            {
                return pooled.Item.UsesThis == false;
            }
        }

        internal sealed class UsesThisWalker : CSharpSyntaxWalker
        {
            private static readonly Pool<UsesThisWalker> Cache = new Pool<UsesThisWalker>(
                () => new UsesThisWalker(),
                x =>
                {
                    x.usesThis = false;
                    x.noThis = false;
                });

            private bool usesThis;
            private bool noThis;

            private UsesThisWalker()
            {
            }

            public bool? UsesThis
            {
                get
                {
                    if (this.usesThis == this.noThis)
                    {
                        return null;
                    }

                    if (this.usesThis && !this.noThis)
                    {
                        return true;
                    }

                    return false;
                }
            }

            public static Pool<UsesThisWalker>.Pooled Create(SyntaxNode node)
            {
                var pooled = Cache.GetOrCreate();
                pooled.Item.Visit(node);
                return pooled;
            }

            public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
            {
                this.CheckUsesThis(node.Left);
                base.VisitAssignmentExpression(node);
            }

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                this.CheckUsesThis(node.Expression);
                base.VisitInvocationExpression(node);
            }

            private void CheckUsesThis(ExpressionSyntax expression)
            {
                if (expression == null)
                {
                    return;
                }

                if ((expression as MemberAccessExpressionSyntax)?.Expression is ThisExpressionSyntax)
                {
                    this.usesThis = true;
                }

                if (expression is IdentifierNameSyntax)
                {
                    this.noThis = true;
                }
            }
        }
    }
}
