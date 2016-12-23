namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;
    using Microsoft.CodeAnalysis.Formatting;
    using Microsoft.CodeAnalysis.Simplification;

    using WpfAnalyzers.PropertyChanged.Helpers;

    internal static class MakePropertyNotifyHelper
    {
        internal static TypeDeclarationSyntax WithBackingField(
            this TypeDeclarationSyntax typeDeclaration,
            PropertyDeclarationSyntax property,
            SyntaxGenerator syntaxGenerator,
            out string fieldName)
        {
            var usesUnderscoreNames = typeDeclaration.UsesUnderscoreNames();
            fieldName = usesUnderscoreNames
                            ? $"_{property.Identifier.ValueText.ToFirstCharLower()}"
                            : property.Identifier.ValueText.ToFirstCharLower();
            while (typeDeclaration.HasMember(fieldName))
            {
                fieldName += "_";
            }

            var field = (FieldDeclarationSyntax)syntaxGenerator.FieldDeclaration(fieldName, property.Type, Accessibility.Private);
            MemberDeclarationSyntax existsingMember;
            if (typeDeclaration.Members.TryGetLast(x => x.IsKind(SyntaxKind.FieldDeclaration), out existsingMember))
            {
                FieldDeclarationSyntax before = null;
                FieldDeclarationSyntax after = null;
                foreach (var member in typeDeclaration.Members)
                {
                    var otherProperty = member as PropertyDeclarationSyntax;
                    if (otherProperty == null || otherProperty == property)
                    {
                        continue;
                    }

                    IdentifierNameSyntax otherField;
                    FieldDeclarationSyntax fieldDeclaration;
                    if (Property.TryGetBackingField(otherProperty, out otherField, out fieldDeclaration))
                    {
                        if (otherProperty.SpanStart < property.SpanStart)
                        {
                            before = fieldDeclaration;
                        }
                        else
                        {
                            after = fieldDeclaration;
                        }
                    }
                }

                if (before != null)
                {
                    return typeDeclaration.InsertNodesAfter(before, new[] { field });
                }

                if (after != null)
                {
                    return typeDeclaration.InsertNodesBefore(after, new[] { field });
                }

                return typeDeclaration.InsertNodesAfter(existsingMember, new[] { field });
            }

            if (typeDeclaration.Members.TryGetFirst(
                x =>
                    x.IsKind(SyntaxKind.ConstructorDeclaration) ||
                    x.IsKind(SyntaxKind.EventFieldDeclaration) ||
                    x.IsKind(SyntaxKind.PropertyDeclaration) ||
                    x.IsKind(SyntaxKind.MethodDeclaration),
                out existsingMember))
            {
                return typeDeclaration.InsertNodesBefore(existsingMember, new[] { field });
            }

            return (TypeDeclarationSyntax)syntaxGenerator.AddMembers(typeDeclaration, field);
        }

        internal static PropertyDeclarationSyntax WithGetterReturningBackingField(this PropertyDeclarationSyntax property, SyntaxGenerator syntaxGenerator, string field)
        {
            var fieldAccess = field.StartsWith("_")
                                    ? field
                                    : $"this.{field}";
            var expressionSyntax = SyntaxFactory.ParseExpression(fieldAccess);
            var returnStatement = syntaxGenerator.ReturnStatement(expressionSyntax);
            return (PropertyDeclarationSyntax)syntaxGenerator.WithGetAccessorStatements(property, new[] { returnStatement }).WithAdditionalAnnotations(Formatter.Annotation);
        }

        internal static PropertyDeclarationSyntax WithNotifyingSetter(this PropertyDeclarationSyntax propertyDeclaration, SyntaxGenerator syntaxGenerator, IPropertySymbol property, string field, IMethodSymbol invoker)
        {
            return WithNotifyingSetter(
                propertyDeclaration,
                syntaxGenerator,
                property,
                syntaxGenerator.AssignValueToBackingField(field),
                field,
                invoker);
        }

        internal static PropertyDeclarationSyntax WithNotifyingSetter(this PropertyDeclarationSyntax propertyDeclaration, SyntaxGenerator syntaxGenerator, IPropertySymbol property, ExpressionStatementSyntax assign, string field, IMethodSymbol invoker)
        {
            var propertyName = propertyDeclaration.Identifier.ValueText;
            var statements = new[]
                                 {
                                     syntaxGenerator.IfValueEqualsBackingFieldReturn(field, property),
                                     assign.WithTrailingTrivia(SyntaxFactory.ElasticMarker),
                                     syntaxGenerator.OnPropertyChanged(propertyName, true, field.StartsWith("_"), invoker),
                                 };
            return (PropertyDeclarationSyntax)syntaxGenerator.WithSetAccessorStatements(propertyDeclaration, statements).WithAdditionalAnnotations(Formatter.Annotation);
        }

        internal static StatementSyntax OnPropertyChanged(this SyntaxGenerator syntaxGenerator, string propertyName, bool useCallerMemberName, bool usedUnderscoreNames, IMethodSymbol invoker)
        {
            var prefix = usedUnderscoreNames
                             ? string.Empty
                             : "this.";
            if (invoker == null)
            {
                var eventAccess = SyntaxFactory.ParseExpression($"{prefix}PropertyChanged?.Invoke(new PropertyChangedEventArgs(nameof({propertyName}))");
                return (StatementSyntax)syntaxGenerator.ExpressionStatement(eventAccess.WithAdditionalAnnotations(Formatter.Annotation))
                                                       .WithAdditionalAnnotations(Formatter.Annotation);
            }

            var memberAccess = SyntaxFactory.ParseExpression($"{prefix}{invoker.Name}");
            if (useCallerMemberName && invoker.Parameters[0].IsCallerMemberName())
            {
                return (StatementSyntax)syntaxGenerator.ExpressionStatement(syntaxGenerator.InvocationExpression(memberAccess));
            }

            var arg = SyntaxFactory.ParseExpression($"nameof({prefix}{propertyName})").WithAdditionalAnnotations(Formatter.Annotation);
            return (StatementSyntax)syntaxGenerator.ExpressionStatement(syntaxGenerator.InvocationExpression(memberAccess, arg))
                                                   .WithAdditionalAnnotations(Formatter.Annotation);
        }

        private static IfStatementSyntax IfValueEqualsBackingFieldReturn(this SyntaxGenerator syntaxGenerator, string fieldName, IPropertySymbol property)
        {
            var fieldAccess = fieldName.StartsWith("_")
                                  ? fieldName
                                  : $"this.{fieldName}";

            if (property.Type.IsValueType || property.Type == KnownSymbol.String)
            {
                if (HasEqualityOperator(property.Type))
                {
                    var valueEqualsExpression = syntaxGenerator.ValueEqualsExpression(
                        SyntaxFactory.ParseName("value"),
                        SyntaxFactory.ParseExpression(fieldAccess));
                    return (IfStatementSyntax)syntaxGenerator.IfStatement(valueEqualsExpression, new[] { SyntaxFactory.ReturnStatement() });
                }

                foreach (var equals in property.Type.GetMembers("Equals"))
                {
                    var method = equals as IMethodSymbol;
                    if (method?.Parameters.Length == 1 && ReferenceEquals(method.Parameters[0].Type, property.Type))
                    {
                        var equalsExpression = syntaxGenerator.InvocationExpression(
                                SyntaxFactory.ParseExpression("value.Equals"),
                                SyntaxFactory.ParseExpression(fieldAccess));
                        return (IfStatementSyntax)syntaxGenerator.IfStatement(equalsExpression, new[] { SyntaxFactory.ReturnStatement() });
                    }
                }

                if (property.Type.Name == "Nullable")
                {
                    if (HasEqualityOperator(((INamedTypeSymbol)property.Type).TypeArguments[0]))
                    {
                        var valueEqualsExpression =
                            syntaxGenerator.ValueEqualsExpression(
                                SyntaxFactory.ParseName("value"),
                                SyntaxFactory.ParseExpression(fieldAccess));
                        return (IfStatementSyntax)syntaxGenerator.IfStatement(valueEqualsExpression, new[] { SyntaxFactory.ReturnStatement() });
                    }

                    var nullableEquals =
                        syntaxGenerator.InvocationExpression(
                            SyntaxFactory.ParseExpression("System.Nullable.Equals").WithAdditionalAnnotations(Simplifier.Annotation),
                            SyntaxFactory.ParseName("value"),
                            SyntaxFactory.ParseExpression(fieldAccess));
                    return (IfStatementSyntax)syntaxGenerator.IfStatement(nullableEquals, new[] { SyntaxFactory.ReturnStatement() });
                }

                var comparerEquals =
                    syntaxGenerator.InvocationExpression(
                        SyntaxFactory.ParseExpression($"System.Collections.Generic.EqualityComparer<{property.Type.ToDisplayString()}>.Default.Equals").WithAdditionalAnnotations(Simplifier.Annotation),
                        SyntaxFactory.ParseName("value"),
                        SyntaxFactory.ParseExpression(fieldAccess));
                return (IfStatementSyntax)syntaxGenerator.IfStatement(comparerEquals, new[] { SyntaxFactory.ReturnStatement() });
            }

            var referenceEqualsExpression = syntaxGenerator.InvocationExpression(
                SyntaxFactory.ParseExpression("ReferenceEquals"),
                SyntaxFactory.ParseName("value"),
                SyntaxFactory.ParseExpression(fieldAccess));
            return (IfStatementSyntax)syntaxGenerator.IfStatement(referenceEqualsExpression, new[] { SyntaxFactory.ReturnStatement() });
        }

        private static bool HasEqualityOperator(ITypeSymbol type)
        {
            switch (type.SpecialType)
            {
                case SpecialType.System_Enum:
                case SpecialType.System_Boolean:
                case SpecialType.System_Char:
                case SpecialType.System_SByte:
                case SpecialType.System_Byte:
                case SpecialType.System_Int16:
                case SpecialType.System_UInt16:
                case SpecialType.System_Int32:
                case SpecialType.System_UInt32:
                case SpecialType.System_Int64:
                case SpecialType.System_UInt64:
                case SpecialType.System_Decimal:
                case SpecialType.System_Single:
                case SpecialType.System_Double:
                case SpecialType.System_String:
                case SpecialType.System_IntPtr:
                case SpecialType.System_UIntPtr:
                case SpecialType.System_DateTime:
                    return true;
            }

            if (type.TypeKind == TypeKind.Enum)
            {
                return true;
            }

            foreach (var op in type.GetMembers("op_Equality"))
            {
                var opMethod = op as IMethodSymbol;
                if (opMethod?.Parameters.Length == 2 &&
                    type.Equals(opMethod.Parameters[0].Type) &&
                    type.Equals(opMethod.Parameters[1].Type))
                {
                    return true;
                }
            }

            return false;
        }

        private static ExpressionStatementSyntax AssignValueToBackingField(this SyntaxGenerator syntaxGenerator, string fieldName)
        {
            var fieldAccess = fieldName.StartsWith("_")
                                  ? fieldName
                                  : $"this.{fieldName}";

            var assignmentStatement = syntaxGenerator.AssignmentStatement(SyntaxFactory.ParseExpression(fieldAccess), SyntaxFactory.ParseName("value"));
            return (ExpressionStatementSyntax)syntaxGenerator.ExpressionStatement(assignmentStatement);
        }

        private static bool HasMember(this TypeDeclarationSyntax typeDeclaration, string name)
        {
            foreach (var member in typeDeclaration.Members)
            {
                var fieldDeclaration = member as BaseFieldDeclarationSyntax;
                if (fieldDeclaration != null)
                {
                    foreach (var variable in fieldDeclaration.Declaration.Variables)
                    {
                        if (variable.Identifier.ValueText == name)
                        {
                            return true;
                        }
                    }

                    continue;
                }

                var property = member as PropertyDeclarationSyntax;
                if (property != null)
                {
                    if (property.Identifier.ValueText == name)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static string ToFirstCharLower(this string text)
        {
            if (char.IsLower(text[0]))
            {
                return text;
            }

            return new string(char.ToLower(text[0]), 1) + text.Substring(1);
        }
    }
}