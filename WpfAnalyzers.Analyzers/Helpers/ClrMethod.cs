namespace WpfAnalyzers
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
        internal static bool IsPotentialClrSetMethod(IMethodSymbol method)
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

        internal static bool IsAttachedSetMethod(IMethodSymbol method, SemanticModel semanticModel, CancellationToken cancellationToken, out BackingFieldOrProperty setField)
        {
            setField = default(BackingFieldOrProperty);
            if (!IsPotentialClrSetMethod(method))
            {
                return false;
            }

            if (method.DeclaringSyntaxReferences.TryGetSingle(out var reference))
            {
                var methodDeclaration = reference.GetSyntax(cancellationToken) as MethodDeclarationSyntax;
                return IsAttachedSetMethod(methodDeclaration, semanticModel, cancellationToken, out _, out setField);
            }

            return false;
        }

        /// <summary>
        /// Check if <paramref name="method"/> is a potential accessor for an attached property
        /// </summary>
        internal static bool IsPotentialClrGetMethod(IMethodSymbol method)
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

        internal static bool IsAttachedGetMethod(IMethodSymbol method, SemanticModel semanticModel, CancellationToken cancellationToken, out BackingFieldOrProperty getField)
        {
            getField = default(BackingFieldOrProperty);
            if (!IsPotentialClrGetMethod(method))
            {
                return false;
            }

            if (method.DeclaringSyntaxReferences.TryGetSingle(out var reference))
            {
                var methodDeclaration = reference.GetSyntax(cancellationToken) as MethodDeclarationSyntax;
                return IsAttachedGetMethod(methodDeclaration, semanticModel, cancellationToken, out _, out getField);
            }

            return false;
        }

        internal static bool IsAttachedSetMethod(MethodDeclarationSyntax method, SemanticModel semanticModel, CancellationToken cancellationToken, out InvocationExpressionSyntax call, out BackingFieldOrProperty setField)
        {
            call = null;
            setField = default(BackingFieldOrProperty);
            if (method == null ||
                method.ParameterList.Parameters.Count != 2 ||
                !method.ReturnType.IsVoid() ||
                !method.Modifiers.Any(SyntaxKind.StaticKeyword))
            {
                return false;
            }

            using (var walker = ClrSetterWalker.Borrow(semanticModel, cancellationToken, method))
            {
                if (!walker.IsSuccess)
                {
                    return false;
                }

                call = walker.SetValue ?? walker.SetCurrentValue;
                var memberAccess = call.Expression as MemberAccessExpressionSyntax;
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

                using (var valueWalker = ValueWalker.Create(method.ParameterList.Parameters[1].Identifier, memberAccess.Parent))
                {
                    if (!valueWalker.UsesValue)
                    {
                        return false;
                    }
                }

                return BackingFieldOrProperty.TryCreate(semanticModel.GetSymbolSafe(walker.Property.Expression, cancellationToken), out setField);
            }
        }

        internal static bool IsAttachedGetMethod(MethodDeclarationSyntax method, SemanticModel semanticModel, CancellationToken cancellationToken, out InvocationExpressionSyntax call, out BackingFieldOrProperty getField)
        {
            call = null;
            getField = default(BackingFieldOrProperty);
            if (method == null ||
                method.ParameterList.Parameters.Count != 1 ||
                method.ReturnType.IsVoid() ||
                !method.Modifiers.Any(SyntaxKind.StaticKeyword))
            {
                return false;
            }

            using (var walker = ClrGetterWalker.Borrow(semanticModel, cancellationToken, method))
            {
                call = walker.GetValue;
                var memberAccess = walker.GetValue?.Expression as MemberAccessExpressionSyntax;
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

                return BackingFieldOrProperty.TryCreate(semanticModel.GetSymbolSafe(walker.Property.Expression, cancellationToken), out getField);
            }
        }

        private class ValueWalker : PooledWalker<ValueWalker>
        {
            private SyntaxToken value;

            public bool UsesValue { get; private set; }

            public static ValueWalker Create(SyntaxToken value, SyntaxNode expression)
            {
                var walker = Borrow(() => new ValueWalker());
                walker.value = value;
                if (expression != null)
                {
                    walker.Visit(expression);
                }

                return walker;
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

            protected override void Clear()
            {
                this.value = default(SyntaxToken);
                this.UsesValue = false;
            }
        }
    }
}