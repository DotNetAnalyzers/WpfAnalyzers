namespace WpfAnalyzers.DependencyProperties
{
    using System.Threading;

    using Microsoft.CodeAnalysis;
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
                !method.Parameters[0].Type.IsAssignableToDependencyObject())
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

            SyntaxReference reference;
            if (method.DeclaringSyntaxReferences.TryGetSingle(out reference))
            {
                var methodDeclaration = reference.GetSyntax(cancellationToken) as MethodDeclarationSyntax;
                return IsAttachedSetMethod(methodDeclaration, semanticModel, cancellationToken, out setField);
            }

            return false;
        }

        internal static bool IsAttachedSetMethod(MethodDeclarationSyntax method, SemanticModel semanticModel, CancellationToken cancellationToken, out IFieldSymbol setField)
        {
            setField = null;
            if (method == null)
            {
                return false;
            }

            using (var setWalker = ClrSetterWalker.Create(semanticModel, cancellationToken, method))
            {
                return setWalker.IsSuccess &&
                       setWalker.Property.TryGetSymbol(semanticModel, cancellationToken, out setField);
            }
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
                !method.Parameters[0].Type.IsAssignableToDependencyObject())
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

            SyntaxReference reference;
            if (method.DeclaringSyntaxReferences.TryGetSingle(out reference))
            {
                var methodDeclaration = reference.GetSyntax(cancellationToken) as MethodDeclarationSyntax;
                return IsAttachedGetMethod(methodDeclaration, semanticModel, cancellationToken, out getField);
            }

            return false;
        }

        internal static bool IsAttachedGetMethod(MethodDeclarationSyntax method, SemanticModel semanticModel, CancellationToken cancellationToken, out IFieldSymbol getField)
        {
            getField = null;
            if (method == null)
            {
                return false;
            }

            using (var setWalker = ClrGetterWalker.Create(semanticModel, cancellationToken, method))
            {
                return setWalker.IsSuccess &&
                       setWalker.Property.TryGetSymbol(semanticModel, cancellationToken, out getField);
            }
        }
    }
}