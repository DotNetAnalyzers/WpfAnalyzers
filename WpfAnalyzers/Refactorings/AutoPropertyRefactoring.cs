namespace WpfAnalyzers.Refactorings
{
    using System.Composition;
    using System.Threading.Tasks;

    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.CodeFixExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeRefactorings;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;
    using Microsoft.CodeAnalysis.Formatting;

    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(AutoPropertyRefactoring))]
    [Shared]
    internal class AutoPropertyRefactoring : CodeRefactoringProvider
    {
        private static readonly UsingDirectiveSyntax SystemWindows = SyntaxFactory.UsingDirective(
            name: SyntaxFactory.QualifiedName(
                left: SyntaxFactory.IdentifierName("System"),
                right: SyntaxFactory.IdentifierName("Windows")));

        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            if (syntaxRoot.FindNode(context.Span) is { } node &&
                node.FirstAncestorOrSelf<PropertyDeclarationSyntax>() is { } property &&
                property.IsAutoProperty())
            {
                if (property.Modifiers.Any(SyntaxKind.StaticKeyword))
                {

                }
                else if (property.Parent is ClassDeclarationSyntax classDeclaration)
                {
                    var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                                         .ConfigureAwait(false);
                    if (semanticModel.TryGetType(classDeclaration, context.CancellationToken, out var containingType) &&
                        containingType.IsAssignableTo(KnownSymbols.DependencyObject, semanticModel.Compilation))
                    {
                        context.RegisterRefactoring(
                            CodeAction.Create(
                                "Change to dependency property",
                                _ => Replace(classDeclaration, WithDependencyProperty(), semanticModel),
                                "Change to dependency property"));

                        SyntaxNode WithDependencyProperty()
                        {
                            var generator = SyntaxGenerator.GetGenerator(context.Document);
                            return generator.AddSorted(
                                classDeclaration.ReplaceNode(
                                    property,
                                    new PropertyRewriter(property.Identifier.ValueText + "Property", null).Visit(property)),
                                Field(
                                    "DependencyProperty",
                                    property.Identifier.ValueText + "Property",
                                    Register("Register", Nameof())));
                        }

                        if (property.Setter() is { } setter &&
                            setter.Modifiers.Any(SyntaxKind.PrivateKeyword))
                        {
                            //context.RegisterRefactoring(
                            //    CodeAction.Create(
                            //        "Change to readonly dependency property",
                            //        _ => Replace(classDeclaration, classDeclaration, semanticModel),
                            //        "Change to readonly dependency property"));
                        }

                        FieldDeclarationSyntax Field(string type, string name, ExpressionSyntax value)
                        {
                            return SyntaxFactory.FieldDeclaration(
                                attributeLists: default,
                                modifiers: SyntaxFactory.TokenList(
                                    SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                                    SyntaxFactory.Token(SyntaxKind.StaticKeyword),
                                    SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)),
                                declaration: SyntaxFactory.VariableDeclaration(
                                    type: SyntaxFactory.IdentifierName(type),
                                    variables: SyntaxFactory.SingletonSeparatedList(
                                        SyntaxFactory.VariableDeclarator(
                                            identifier: SyntaxFactory.Identifier(name),
                                            argumentList: default,
                                            initializer: SyntaxFactory.EqualsValueClause(
                                                value: value)))),
                                semicolonToken: SyntaxFactory.Token(SyntaxKind.SemicolonToken));
                        }

                        InvocationExpressionSyntax Register(string methodName, ExpressionSyntax name)
                        {
                            return SyntaxFactory.InvocationExpression(
                                expression: SyntaxFactory.MemberAccessExpression(
                                    kind: SyntaxKind.SimpleMemberAccessExpression,
                                    expression: SyntaxFactory.IdentifierName("DependencyProperty"),
                                    name: SyntaxFactory.IdentifierName(methodName)),
                                argumentList: SyntaxFactory.ArgumentList(
                                    openParenToken: SyntaxFactory.Token(
                                        leading: default,
                                        kind: SyntaxKind.OpenParenToken,
                                        trailing: SyntaxFactory.TriviaList(SyntaxFactory.LineFeed)),
                                    arguments: SyntaxFactory.SeparatedList(
                                        new[]
                                        {
                                                SyntaxFactory.Argument(expression: name),
                                                SyntaxFactory.Argument(
                                                    nameColon: default,
                                                    refKindKeyword: default,
                                                    expression: SyntaxFactory.TypeOfExpression(
                                                        keyword: SyntaxFactory.Token(
                                                            leading: SyntaxFactory.TriviaList(SyntaxFactory.Whitespace("            ")),
                                                            kind: SyntaxKind.TypeOfKeyword,
                                                            trailing: default),
                                                        openParenToken: SyntaxFactory.Token(SyntaxKind.OpenParenToken),
                                                        type: property.Type,
                                                        closeParenToken: SyntaxFactory.Token(SyntaxKind.CloseParenToken))),
                                                SyntaxFactory.Argument(
                                                    nameColon: default,
                                                    refKindKeyword: default,
                                                    expression: SyntaxFactory.TypeOfExpression(
                                                        keyword: SyntaxFactory.Token(
                                                            leading: SyntaxFactory.TriviaList(SyntaxFactory.Whitespace("            ")),
                                                            kind: SyntaxKind.TypeOfKeyword,
                                                            trailing: default),
                                                        openParenToken: SyntaxFactory.Token(SyntaxKind.OpenParenToken),
                                                        type: SyntaxFactory.IdentifierName(classDeclaration.Identifier),
                                                        closeParenToken: SyntaxFactory.Token(SyntaxKind.CloseParenToken))),
                                                SyntaxFactory.Argument(
                                                    nameColon: default,
                                                    refKindKeyword: default,
                                                    expression: SyntaxFactory.ObjectCreationExpression(
                                                        newKeyword: SyntaxFactory.Token(
                                                            leading: SyntaxFactory.TriviaList(SyntaxFactory.Whitespace("            ")),
                                                            kind: SyntaxKind.NewKeyword,
                                                            trailing: SyntaxFactory.TriviaList(SyntaxFactory.Space)),
                                                        type: SyntaxFactory.IdentifierName("PropertyMetadata"),
                                                        argumentList: SyntaxFactory.ArgumentList(
                                                            arguments: SyntaxFactory.SingletonSeparatedList(
                                                                SyntaxFactory.Argument(expression: SyntaxFactory.DefaultExpression(property.Type)))),
                                                        initializer: default)),
                                        },
                                        new[]
                                        {
                                                SyntaxFactory.Token(
                                                    leading: default,
                                                    kind: SyntaxKind.CommaToken,
                                                    trailing: SyntaxFactory.TriviaList(SyntaxFactory.LineFeed)),
                                                SyntaxFactory.Token(
                                                    leading: default,
                                                    kind: SyntaxKind.CommaToken,
                                                    trailing: SyntaxFactory.TriviaList(SyntaxFactory.LineFeed)),
                                                SyntaxFactory.Token(
                                                    leading: default,
                                                    kind: SyntaxKind.CommaToken,
                                                    trailing: SyntaxFactory.TriviaList(SyntaxFactory.LineFeed)),
                                        }),
                                    closeParenToken: SyntaxFactory.Token(SyntaxKind.CloseParenToken)));
                        }

                        InvocationExpressionSyntax Nameof()
                        {
                            return SyntaxFactory.InvocationExpression(
                                expression: SyntaxFactory.IdentifierName(
                                    identifier: SyntaxFactory.Identifier(
                                        leading: SyntaxFactory.TriviaList(SyntaxFactory.Whitespace("            ")),
                                        text: "nameof",
                                        trailing: default)),
                                argumentList: SyntaxFactory.ArgumentList(
                                    arguments: SyntaxFactory.SingletonSeparatedList(
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.IdentifierName(identifier: property.Identifier)))));
                        }
                    }
                }

                Task<Document> Replace(ClassDeclarationSyntax node, SyntaxNode replacement, SemanticModel semanticModel)
                {
                    if (syntaxRoot is CompilationUnitSyntax compilationUnit)
                    {
                        return Task.FromResult(
                            context.Document.WithSyntaxRoot(
                                compilationUnit.ReplaceNode(node, replacement)
                                               .AddUsing(SystemWindows, semanticModel)));
                    }

                    return Task.FromResult(context.Document.WithSyntaxRoot(syntaxRoot.ReplaceNode(node, replacement)));
                }
            }
        }

        private class PropertyRewriter : CSharpSyntaxRewriter
        {
            private readonly string dependencyPropertyName;
            private readonly string? dependencyPropertyKeyName;

            internal PropertyRewriter(string dependencyPropertyName, string? dependencyPropertyKeyName)
            {
                this.dependencyPropertyName = dependencyPropertyName;
                this.dependencyPropertyKeyName = dependencyPropertyKeyName;
            }

            public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
            {
                return base.VisitPropertyDeclaration(node).WithAdditionalAnnotations(Formatter.Annotation);
            }

            public override SyntaxNode VisitAccessorList(AccessorListSyntax node)
            {
                return base.VisitAccessorList(node)
                           .WithLeadingTrivia(SyntaxFactory.LineFeed, SyntaxFactory.Whitespace("        "));
            }

            public override SyntaxNode VisitAccessorDeclaration(AccessorDeclarationSyntax node)
            {
                return node.Kind() switch
                {
                    SyntaxKind.GetAccessorDeclaration => node.WithExpressionBody(
                        SyntaxFactory.ArrowExpressionClause(
                            SyntaxFactory.CastExpression(
                                Type(),
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.ThisExpression(),
                                        SyntaxFactory.IdentifierName("GetValue")),
                                    SyntaxFactory.ArgumentList(
                                        SyntaxFactory.SingletonSeparatedList(
                                            SyntaxFactory.Argument(SyntaxFactory.IdentifierName(this.dependencyPropertyName))))))))
                                                             .WithLeadingTrivia(SyntaxFactory.LineFeed, SyntaxFactory.Whitespace("            ")),
                    SyntaxKind.SetAccessorDeclaration => node.WithExpressionBody(
                        SyntaxFactory.ArrowExpressionClause(
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.ThisExpression(),
                                    SyntaxFactory.IdentifierName("SetValue")),
                                SyntaxFactory.ArgumentList(
                                    SyntaxFactory.SeparatedList(
                                        new[]
                                        {
                                            SyntaxFactory.Argument(SyntaxFactory.IdentifierName(this.dependencyPropertyKeyName ?? this.dependencyPropertyName)),
                                            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("value")),
                                        })))))
                                                             .WithLeadingTrivia(SyntaxFactory.LineFeed, SyntaxFactory.Whitespace("            "))
                                                             .WithTrailingTrivia(SyntaxFactory.LineFeed, SyntaxFactory.Whitespace("        ")),
                    _ => node,
                };

                TypeSyntax Type() => ((PropertyDeclarationSyntax)node.Parent.Parent).Type;
            }
        }
    }
}
