namespace WpfAnalyzers.PropertyChanged.Helpers
{
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class Property
    {
        internal static bool ShouldNotify(PropertyDeclarationSyntax declaration, IPropertySymbol propertySymbol, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (propertySymbol.IsIndexer ||
                propertySymbol.DeclaredAccessibility != Accessibility.Public ||
                propertySymbol.IsStatic ||
                propertySymbol.IsReadOnly ||
                propertySymbol.IsAbstract ||
                propertySymbol.ContainingType.IsValueType ||
                propertySymbol.ContainingType.DeclaredAccessibility != Accessibility.Public)
            {
                return false;
            }

            if (IsMutableAutoProperty(declaration))
            {
                return true;
            }

            AccessorDeclarationSyntax setter;
            if (declaration.TryGetSetAccessorDeclaration(out setter))
            {
                if (!AssignsValueToBackingField(setter))
                {
                    return false;
                }

                if (setter.InvokesPropertyChangedFor(propertySymbol, semanticModel, cancellationToken) != PropertyChanged.InvokesPropertyChanged.No)
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        internal static bool IsMutableAutoProperty(PropertyDeclarationSyntax property)
        {
            var accessors = property?.AccessorList?.Accessors;
            if (accessors?.Count != 2)
            {
                return false;
            }

            foreach (var accessor in accessors.Value)
            {
                if (accessor.Body != null)
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool TryGetBackingField(IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken, out IFieldSymbol field)
        {
            foreach (var declaration in property.Declarations(cancellationToken))
            {
                SemanticModel fieldDeclaration;
                if (TryGetBackingField(declaration, out fieldDeclaration))
                {
                    
                }
            }
        }

        internal static bool TryGetBackingField(PropertyDeclarationSyntax property, out IdentifierNameSyntax field)
        {
            field = null;

            AccessorDeclarationSyntax setter;
            if (property.TryGetSetAccessorDeclaration(out setter) &&
                setter.Body != null)
            {
                using (var pooled = AssignmentWalker.Create(setter))
                {
                    if (pooled.Item.Assignments.Count != 1)
                    {
                        return false;
                    }

                    var left = pooled.Item.Assignments[0].Left;
                    field = left as IdentifierNameSyntax;
                    if (field == null)
                    {
                        var memberAccess = left as MemberAccessExpressionSyntax;
                        if (!(memberAccess?.Expression is ThisExpressionSyntax))
                        {
                            return false;
                        }

                        field = memberAccess.Name as IdentifierNameSyntax;
                    }

                    if (field == null)
                    {
                        return false;
                    }

                    var typeDeclaration = property.FirstAncestorOrSelf<TypeDeclarationSyntax>();
                    foreach (var member in typeDeclaration.Members)
                    {
                        field = member as FieldDeclarationSyntax;
                        if (field == null)
                        {
                            continue;
                        }

                        foreach (var variable in field.Declaration.Variables)
                        {
                            if (variable.Identifier.ValueText == name)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        internal static bool AssignsValueToBackingField(AccessorDeclarationSyntax setter)
        {
            using (var pooled = AssignmentWalker.Create(setter))
            {
                foreach (var assignment in pooled.Item.Assignments)
                {
                    if ((assignment.Right as IdentifierNameSyntax)?.Identifier.ValueText != "value")
                    {
                        continue;
                    }

                    if (assignment.Left is IdentifierNameSyntax)
                    {
                        return true;
                    }

                    var memberAccess = assignment.Left as MemberAccessExpressionSyntax;
                    if (memberAccess?.Expression is ThisExpressionSyntax &&
                        memberAccess.Name is IdentifierNameSyntax)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}