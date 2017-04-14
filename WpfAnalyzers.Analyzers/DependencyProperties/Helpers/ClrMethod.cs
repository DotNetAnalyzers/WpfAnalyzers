namespace WpfAnalyzers.DependencyProperties
{
    using System.Threading;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class ClrMethod
    {
        /// <summary>
        /// Check if <paramref name="method"/> is a potential accessor for an attached property
        /// </summary>
        internal static bool IsPotentialClrSetMethod(this IMethodSymbol method)
        {
            if (method == null)
            {
                return false;
            }

            if (!method.IsStatic ||
                method.Parameters.Length != 2 ||
                method.AssociatedSymbol != null ||
                !method.Parameters[0].Type.Is(KnownSymbol.DependencyObject) ||
                method.Parameters[1].Type == KnownSymbol.DependencyPropertyChangedEventArgs ||
                !method.ReturnsVoid ||
                !method.Name.StartsWith("Set"))
            {
                return false;
            }

            return true;
        }

        internal static bool IsAttachedSetMethod(IMethodSymbol method, SemanticModel semanticModel, CancellationToken cancellationToken, out IFieldSymbol setField)
        {
            setField = null;
            if (!IsPotentialClrSetMethod(method))
            {
                return false;
            }

            if (method.DeclaringSyntaxReferences.TryGetSingle(out SyntaxReference reference))
            {
                var methodDeclaration = reference.GetSyntax(cancellationToken) as MethodDeclarationSyntax;
                return IsAttachedSetMethod(methodDeclaration, semanticModel, cancellationToken, out setField);
            }

            return false;
        }

        /// <summary>
        /// Check if <paramref name="method"/> is a potential accessor for an attached property
        /// </summary>
        internal static bool IsPotentialClrGetMethod(this IMethodSymbol method)
        {
            if (method == null)
            {
                return false;
            }

            if (!method.IsStatic ||
                method.Parameters.Length != 1 ||
                !method.Parameters[0].Type.Is(KnownSymbol.DependencyObject) ||
                method.ReturnsVoid ||
                !method.Name.StartsWith("Get"))
            {
                return false;
            }

            return true;
        }

        internal static bool IsAttachedGetMethod(IMethodSymbol method, SemanticModel semanticModel, CancellationToken cancellationToken, out IFieldSymbol getField)
        {
            getField = null;
            if (!IsPotentialClrGetMethod(method))
            {
                return false;
            }

            if (method.DeclaringSyntaxReferences.TryGetSingle(out SyntaxReference reference))
            {
                var methodDeclaration = reference.GetSyntax(cancellationToken) as MethodDeclarationSyntax;
                return IsAttachedGetMethod(methodDeclaration, semanticModel, cancellationToken, out getField);
            }

            return false;
        }

        private static bool IsAttachedSetMethod(MethodDeclarationSyntax method, SemanticModel semanticModel, CancellationToken cancellationToken, out IFieldSymbol setField)
        {
            setField = null;
            if (method == null ||
                method.ParameterList.Parameters.Count != 2 ||
                !method.ReturnType.IsVoid() ||
                !method.Modifiers.Any(SyntaxKind.StaticKeyword))
            {
                return false;
            }

            using (var pooled = ClrSetterWalker.Create(semanticModel, cancellationToken, method))
            {
                if (!pooled.Item.IsSuccess)
                {
                    return false;
                }

                var memberAccess = (pooled.Item.SetValue?.Expression ?? pooled.Item.SetCurrentValue?.Expression) as MemberAccessExpressionSyntax;
                var member = memberAccess?.Expression as IdentifierNameSyntax;
                if (memberAccess == null ||
                    member == null ||
                    !memberAccess.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                {
                    return false;
                }

                if (method.ParameterList.Parameters[0].Identifier.ValueText != member.Identifier.ValueText)
                {
                    return false;
                }

                using (var pooledValueWalker = ValueWalker.Create(method.ParameterList.Parameters[1].Identifier, memberAccess.Parent))
                {
                    if (!pooledValueWalker.Item.UsesValue)
                    {
                        return false;
                    }
                }

                return pooled.Item.Property.TryGetSymbol(semanticModel, cancellationToken, out setField);
            }
        }

        private static bool IsAttachedGetMethod(MethodDeclarationSyntax method, SemanticModel semanticModel, CancellationToken cancellationToken, out IFieldSymbol getField)
        {
            getField = null;
            if (method == null ||
                method.ParameterList.Parameters.Count != 1 ||
                method.ReturnType.IsVoid() ||
                !method.Modifiers.Any(SyntaxKind.StaticKeyword))
            {
                return false;
            }

            using (var pooled = ClrGetterWalker.Create(semanticModel, cancellationToken, method))
            {
                var memberAccess = pooled.Item.GetValue?.Expression as MemberAccessExpressionSyntax;
                var member = memberAccess?.Expression as IdentifierNameSyntax;
                if (memberAccess == null ||
                    member == null ||
                    !memberAccess.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                {
                    return false;
                }

                if (method.ParameterList.Parameters[0].Identifier.ValueText != member.Identifier.ValueText)
                {
                    return false;
                }

                return pooled.Item.Property.TryGetSymbol(semanticModel, cancellationToken, out getField);
            }
        }

        private class ValueWalker : CSharpSyntaxWalker
        {
            private static readonly Pool<ValueWalker> Cache = new Pool<ValueWalker>(
                () => new ValueWalker(),
                x =>
                {
                    x.value = default(SyntaxToken);
                    x.UsesValue = false;
                });

            private SyntaxToken value;

            public bool UsesValue { get; private set; }

            public static Pool<ValueWalker>.Pooled Create(SyntaxToken value, SyntaxNode expression)
            {
                var pooled = Cache.GetOrCreate();
                pooled.Item.value = value;
                if (expression != null)
                {
                    pooled.Item.Visit(expression);
                }

                return pooled;
            }

            public override void Visit(SyntaxNode node)
            {
                if (this.UsesValue)
                {
                    return;
                }

                base.Visit(node);
            }

            public override void VisitIdentifierName(IdentifierNameSyntax node)
            {
                this.UsesValue |= this.value.ValueText == node.Identifier.ValueText;
                base.VisitIdentifierName(node);
            }
        }
    }
}