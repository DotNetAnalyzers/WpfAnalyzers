namespace WpfAnalyzers
{
    using System.Linq;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class PropertySymbolExt
    {
        internal static bool TryGetSetterSyntax(this IPropertySymbol property, out AccessorDeclarationSyntax setter)
        {
            setter = null;
            if (property?.SetMethod.DeclaringSyntaxReferences.Length != 1)
            {
                return false;
            }

            var reference = property.SetMethod.DeclaringSyntaxReferences[0];
            setter = reference.SyntaxTree.GetRoot().FindNode(reference.Span) as AccessorDeclarationSyntax;
            return setter != null;
        }

        internal static bool IsPotentialDependencyPropertyAccessor(this IPropertySymbol property)
        {
            return property != null &&
                   !property.IsIndexer &&
                   !property.IsReadOnly &&
                   !property.IsWriteOnly &&
                   !property.IsStatic &&
                   property.ContainingType.IsAssignableToDependencyObject();
        }

        internal static bool TryGetMutableDependencyPropertyField(this IPropertySymbol property, out IFieldSymbol result)
        {
            result = null;
            if (!property.IsPotentialDependencyPropertyAccessor())
            {
                return false;
            }

            foreach (var name in property.ContainingType.MemberNames)
            {
                if (name.IsParts(property.Name, "Property"))
                {
                    result = property.ContainingType
                                     .GetMembers(name)
                                     .OfType<IFieldSymbol>()
                                     .FirstOrDefault();
                }

                if (name.IsParts(property.Name, "PropertyKey"))
                {
                    return false;
                }
            }

            return result != null;
        }
    }
}
