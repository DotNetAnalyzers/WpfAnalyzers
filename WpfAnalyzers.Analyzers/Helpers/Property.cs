namespace WpfAnalyzers
{
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class Property
    {
        internal static bool IsLazy(this PropertyDeclarationSyntax propertyDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (propertyDeclaration.TryGetSetAccessorDeclaration(out AccessorDeclarationSyntax _))
            {
                return false;
            }

            IFieldSymbol returnedField = null;
            if (propertyDeclaration.TryGetGetAccessorDeclaration(out var getter))
            {
                if (getter.Body == null)
                {
                    return false;
                }

                using (var walker = ReturnExpressionsWalker.Create(getter.Body))
                {
                    if (walker.ReturnValues.Count == 0)
                    {
                        return false;
                    }

                    foreach (var returnValue in walker.ReturnValues)
                    {
                        var returnedSymbol = returnValue?.IsKind(SyntaxKind.CoalesceExpression) == true
                            ? semanticModel.GetSymbolSafe((returnValue as BinaryExpressionSyntax)?.Left, cancellationToken) as IFieldSymbol
                            : semanticModel.GetSymbolSafe(returnValue, cancellationToken) as IFieldSymbol;
                        if (returnedSymbol == null)
                        {
                            return false;
                        }

                        if (returnedField != null &&
                            !ReferenceEquals(returnedSymbol, returnedField))
                        {
                            return false;
                        }

                        returnedField = returnedSymbol;
                    }
                }

                return AssignmentWalker.Assigns(returnedField, getter.Body, semanticModel, cancellationToken);
            }

            var arrow = propertyDeclaration.ExpressionBody;
            if (arrow?.Expression?.IsKind(SyntaxKind.CoalesceExpression) != true)
            {
                return false;
            }

            returnedField = semanticModel.GetSymbolSafe((arrow.Expression as BinaryExpressionSyntax)?.Left, cancellationToken) as IFieldSymbol;
            return AssignmentWalker.Assigns(returnedField, arrow.Expression, semanticModel, cancellationToken);
        }

        internal static bool ShouldNotify(PropertyDeclarationSyntax declaration, IPropertySymbol propertySymbol, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (propertySymbol.IsIndexer ||
                propertySymbol.DeclaredAccessibility != Accessibility.Public ||
                propertySymbol.IsStatic ||
                propertySymbol.IsReadOnly ||
                propertySymbol.GetMethod == null ||
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

            if (declaration.TryGetSetAccessorDeclaration(out var setter))
            {
                if (!AssignsValueToBackingField(setter, out var assignment))
                {
                    return false;
                }

                if (PropertyChanged.InvokesPropertyChangedFor(assignment, propertySymbol, semanticModel, cancellationToken) != AnalysisResult.No)
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

        internal static bool IsSimplePropertyWithBackingField(PropertyDeclarationSyntax property, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (!(property.TryGetGetAccessorDeclaration(out var getter) &&
property.TryGetSetAccessorDeclaration(out var setter)))
            {
                return false;
            }

            if (getter.Body?.Statements.Count != 1 ||
                setter.Body?.Statements.Count != 1)
            {
                return false;
            }

            var returnStatement = getter.Body.Statements[0] as ReturnStatementSyntax;
            var assignment = (setter.Body.Statements[0] as ExpressionStatementSyntax)?.Expression as AssignmentExpressionSyntax;
            if (returnStatement == null ||
                assignment == null)
            {
                return false;
            }

            var returnedField = semanticModel.GetSymbolSafe(returnStatement.Expression, cancellationToken) as IFieldSymbol;
            var assignedField = semanticModel.GetSymbolSafe(assignment.Left, cancellationToken) as IFieldSymbol;
            if (assignedField == null ||
                returnedField == null)
            {
                return false;
            }

            var propertySymbol = semanticModel.GetDeclaredSymbolSafe(property, cancellationToken);
            return assignedField.Equals(returnedField) && assignedField.ContainingType == propertySymbol?.ContainingType;
        }

        internal static bool TryGetBackingField(IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken, out IFieldSymbol field)
        {
            field = null;
            if (property == null)
            {
                return false;
            }

            foreach (var declaration in property.Declarations(cancellationToken))
            {
                var propertyDeclaration = declaration as PropertyDeclarationSyntax;
                if (propertyDeclaration == null)
                {
                    continue;
                }

                if (TryGetBackingField(propertyDeclaration, out var fieldIdentifier, out FieldDeclarationSyntax _))
                {
                    field = semanticModel.GetSymbolSafe(fieldIdentifier, cancellationToken) as IFieldSymbol;
                    return field != null;
                }
            }

            return false;
        }

        internal static bool TryGetBackingField(PropertyDeclarationSyntax property, out IdentifierNameSyntax field, out FieldDeclarationSyntax fieldDeclaration)
        {
            field = null;
            fieldDeclaration = null;
            if (property == null)
            {
                return false;
            }

            if (property.TryGetSetAccessorDeclaration(out var setter) &&
    setter.Body != null)
            {
                using (var walker = AssignmentWalker.Borrow(setter))
                {
                    if (walker.Assignments.Count != 1)
                    {
                        return false;
                    }

                    var left = walker.Assignments[0].Left;
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

                    foreach (var member in property.FirstAncestorOrSelf<TypeDeclarationSyntax>().Members)
                    {
                        fieldDeclaration = member as FieldDeclarationSyntax;
                        if (fieldDeclaration != null)
                        {
                            foreach (var variable in fieldDeclaration.Declaration.Variables)
                            {
                                if (variable.Identifier.ValueText == field.Identifier.ValueText)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        internal static bool AssignsValueToBackingField(AccessorDeclarationSyntax setter, out AssignmentExpressionSyntax assignment)
        {
            using (var pooled = AssignmentWalker.Borrow(setter))
            {
                foreach (var a in pooled.Assignments)
                {
                    if ((a.Right as IdentifierNameSyntax)?.Identifier.ValueText != "value")
                    {
                        continue;
                    }

                    if (a.Left is IdentifierNameSyntax)
                    {
                        assignment = a;
                        return true;
                    }

                    if (a.Left is MemberAccessExpressionSyntax memberAccess &&
                        memberAccess.Name is IdentifierNameSyntax)
                    {
                        if (memberAccess.Expression is ThisExpressionSyntax ||
                            memberAccess.Expression is IdentifierNameSyntax)
                        {
                            assignment = a;
                            return true;
                        }

                        if (memberAccess.Expression is MemberAccessExpressionSyntax nested &&
                            nested.Expression is ThisExpressionSyntax &&
                            nested.Name is IdentifierNameSyntax)
                        {
                            assignment = a;
                            return true;
                        }
                    }
                }
            }

            assignment = null;
            return false;
        }

        internal static bool TryFindValue(AccessorDeclarationSyntax setter, SemanticModel semanticModel, CancellationToken cancellationToken, out IParameterSymbol value)
        {
            using (var walker = IdentifierNameWalker.Borrow(setter))
            {
                foreach (var identifierName in walker.IdentifierNames)
                {
                    if (identifierName.Identifier.ValueText == "value")
                    {
                        value = semanticModel.GetSymbolSafe(identifierName, cancellationToken) as IParameterSymbol;
                        if (value != null)
                        {
                            return true;
                        }
                    }
                }
            }

            value = null;
            return false;
        }
    }
}