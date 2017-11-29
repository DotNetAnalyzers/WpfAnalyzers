namespace WpfAnalyzers
{
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class AttachedPropertyBrowsableForType
    {
        internal static bool TryGetAttribute(MethodDeclarationSyntax methodDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken, out AttributeSyntax result)
        {
            result = null;
            if (methodDeclaration == null)
            {
                return false;
            }

            foreach (var attributeList in methodDeclaration.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    if (Attribute.IsType(
                        attribute,
                        KnownSymbol.AttachedPropertyBrowsableForTypeAttribute,
                        semanticModel,
                        cancellationToken))
                    {
                        result = attribute;
                        return true;
                    }
                }
            }

            return false;
        }

        internal static bool TryGetType(MethodDeclarationSyntax methodDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken, out ITypeSymbol type)
        {
            type = null;
            if (methodDeclaration == null ||
                methodDeclaration.ParameterList == null ||
                methodDeclaration.ParameterList.Parameters.Count != 1)
            {
                return false;
            }

            type = semanticModel.GetDeclaredSymbolSafe(methodDeclaration, cancellationToken)
                                .Parameters[0]
                                .Type;
            return true;
        }
    }
}