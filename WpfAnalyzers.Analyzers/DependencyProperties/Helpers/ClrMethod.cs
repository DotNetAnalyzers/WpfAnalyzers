namespace WpfAnalyzers.DependencyProperties
{
    using System.Threading;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class ClrMethod
    {
        internal static bool IsAttachedSetMethod(IMethodSymbol method, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (!method.IsStatic ||
                method.Parameters.Length != 2 ||
                !method.Parameters[0].Type.IsAssignableToDependencyObject())
            {
                return false;
            }

            SyntaxReference reference;
            if (method.DeclaringSyntaxReferences.TryGetSingle(out reference))
            {
                var methodDeclaration = reference.GetSyntax(cancellationToken) as MethodDeclarationSyntax;
                using (var setWalker = ClrSetterWalker.Create(semanticModel, cancellationToken, methodDeclaration))
                {
                    return setWalker.IsSuccess;
                }
            }

            return false;
        }

        internal static bool IsAttachedSetAccessor(MethodDeclarationSyntax method, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            using (var setWalker = ClrSetterWalker.Create(semanticModel, cancellationToken, method))
            {
                return setWalker.IsSuccess;
            }
        }
    }
}