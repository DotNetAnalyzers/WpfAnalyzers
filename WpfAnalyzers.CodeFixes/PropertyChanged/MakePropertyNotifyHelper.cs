namespace WpfAnalyzers
{
    using System;

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

                    FieldDeclarationSyntax otherField;
                    if (Property.TryGetBackingField(otherProperty, out otherField))
                    {
                        if (otherProperty.SpanStart < property.SpanStart)
                        {
                            before = otherField;
                        }
                        else
                        {
                            after = otherField;
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
            var statements = new[]
                                 {
                                     syntaxGenerator.IfValueEqualsBackingFieldReturn(field, property),
                                     assign.WithTrailingTrivia(SyntaxFactory.ElasticMarker),
                                     syntaxGenerator.Invoke(propertyDeclaration, field.StartsWith("_"), invoker),
                                 };
            return (PropertyDeclarationSyntax)syntaxGenerator.WithSetAccessorStatements(propertyDeclaration, statements).WithAdditionalAnnotations(Formatter.Annotation);
        }

        private static bool UsesUnderscoreNames(this TypeDeclarationSyntax type)
        {
            foreach (var member in type.Members)
            {
                var field = member as FieldDeclarationSyntax;
                if (field == null)
                {
                    continue;
                }

                foreach (var variable in field.Declaration.Variables)
                {
                    return variable.Identifier.ValueText.StartsWith("_");
                }
            }

            return false;
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

            return type.GetMembers("op_Equality").Length == 1 || type.TypeKind == TypeKind.Enum;
        }

        private static ExpressionStatementSyntax AssignValueToBackingField(this SyntaxGenerator syntaxGenerator, string fieldName)
        {
            var fieldAccess = fieldName.StartsWith("_")
                                  ? fieldName
                                  : $"this.{fieldName}";

            var assignmentStatement = syntaxGenerator.AssignmentStatement(SyntaxFactory.ParseExpression(fieldAccess), SyntaxFactory.ParseName("value"));
            return (ExpressionStatementSyntax)syntaxGenerator.ExpressionStatement(assignmentStatement);
        }

        private static StatementSyntax Invoke(this SyntaxGenerator syntaxGenerator, PropertyDeclarationSyntax property, bool usedUnderscoreNames, IMethodSymbol invoker)
        {
            var prefix = usedUnderscoreNames
                             ? string.Empty
                             : "this.";
            var memberAccess = SyntaxFactory.ParseExpression($"{prefix}{invoker.Name}");
            if (invoker.Parameters[0].IsCallerMemberName())
            {
                return (StatementSyntax)syntaxGenerator.ExpressionStatement(syntaxGenerator.InvocationExpression(memberAccess));
            }

            var arg = SyntaxFactory.ParseExpression($"nameof({prefix}{property.Identifier.ValueText})");
            return (StatementSyntax)syntaxGenerator.ExpressionStatement(syntaxGenerator.InvocationExpression(memberAccess, arg));
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