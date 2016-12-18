namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;

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

        internal static PropertyDeclarationSyntax WithGeterReturningBackingField(this PropertyDeclarationSyntax property, SyntaxGenerator syntaxGenerator, string field)
        {
            var returnStatement = field.StartsWith("_")
                                      ? syntaxGenerator.ReturnStatement(SyntaxFactory.ParseExpression(field))
                                      : syntaxGenerator.ReturnStatement(SyntaxFactory.ParseExpression($"this.{field}"));
            return (PropertyDeclarationSyntax)syntaxGenerator.WithGetAccessorStatements(property, new[] { returnStatement });
        }

        internal static PropertyDeclarationSyntax WithNotifyingSetter(this PropertyDeclarationSyntax property, SyntaxGenerator syntaxGenerator, string field, IMethodSymbol invoker)
        {
            var statements = new[]
                                 {
                                     syntaxGenerator.IfValueEqualsBackingFieldReturn(field),
                                     syntaxGenerator.AssignValueToBackingField(field),
                                     syntaxGenerator.Invoke(property, field.StartsWith("_"), invoker),
                                 };
            return (PropertyDeclarationSyntax)syntaxGenerator.WithSetAccessorStatements(property, statements);
        }

        internal static bool UsesUnderscoreNames(this TypeDeclarationSyntax type)
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
                    if (variable.Identifier.ValueText.StartsWith("_"))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static IfStatementSyntax IfValueEqualsBackingFieldReturn(this SyntaxGenerator syntaxGenerator, string fieldName)
        {
            var fieldAccess = fieldName.StartsWith("_")
                                  ? fieldName
                                  : $"this.{fieldName}";

            return (IfStatementSyntax)syntaxGenerator.IfStatement(
                SyntaxFactory.ParseExpression($"value == {fieldAccess}"),
                new[] { SyntaxFactory.ReturnStatement() });
        }

        private static StatementSyntax AssignValueToBackingField(this SyntaxGenerator syntaxGenerator, string fieldName)
        {
            var fieldAccess = fieldName.StartsWith("_")
                                  ? fieldName
                                  : $"this.{fieldName}";

            return SyntaxFactory.ParseStatement($"{fieldAccess} = value;");
        }

        // ReSharper disable once UnusedParameter.Local
        private static StatementSyntax Invoke(this SyntaxGenerator syntaxGenerator, PropertyDeclarationSyntax property, bool usedUnderscoreNames, IMethodSymbol invoker)
        {
            var prefix = usedUnderscoreNames
                             ? string.Empty
                             : "this.";
            var arg = $"nameof({property.Identifier.ValueText})";
            return SyntaxFactory.ParseStatement($"{prefix}{invoker.Name}({arg});");
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

                    continue;
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