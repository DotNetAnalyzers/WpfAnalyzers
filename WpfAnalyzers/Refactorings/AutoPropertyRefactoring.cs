namespace WpfAnalyzers.Refactorings
{
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;

    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.CodeFixExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeRefactorings;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

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
                property.IsAutoProperty() &&
                property.Parent is ClassDeclarationSyntax containingClass)
            {
                if (property.Modifiers.Any(SyntaxKind.StaticKeyword))
                {

                }
                else
                {
                    var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                                         .ConfigureAwait(false);
                    if (semanticModel.TryGetType(containingClass, context.CancellationToken, out var containingType) &&
                        containingType.IsAssignableTo(KnownSymbols.DependencyObject, semanticModel.Compilation))
                    {
                        context.RegisterRefactoring(
                            CodeAction.Create(
                                "Change to dependency property",
                                c => WithDependencyProperty(c),
                                "Change to dependency property"));

                        async Task<Document> WithDependencyProperty(CancellationToken cancellationToken)
                        {
                            var qualifyMethodAccess = await context.Document.QualifyMethodAccessAsync(cancellationToken)
                                                                        .ConfigureAwait(false);
                            var updatedClass = containingClass.ReplaceNode(
                                                                  property,
                                                                  Property(
                                                                      property.Identifier.ValueText + "Property",
                                                                      qualifyMethodAccess != CodeStyleResult.No))
                                                              .AddField(
                                                                  Field(
                                                                      KnownSymbols.DependencyProperty,
                                                                      property,
                                                                      Register("Register", Nameof()),
                                                                      semanticModel));

                            if (syntaxRoot is CompilationUnitSyntax compilationUnit)
                            {
                                return context.Document.WithSyntaxRoot(
                                        compilationUnit.ReplaceNode(containingClass, updatedClass)
                                                       .AddUsing(SystemWindows, semanticModel));
                            }

                            return context.Document.WithSyntaxRoot(syntaxRoot.ReplaceNode(containingClass, updatedClass));

                            PropertyDeclarationSyntax Property(string field, bool qualifyMethodAccess)
                            {
                                return property.WithIdentifier(property.Identifier.WithTrailingTrivia(SyntaxFactory.LineFeed))
                                               .WithAccessorList(
                                    SyntaxFactory.AccessorList(
                                        openBraceToken: SyntaxFactory.Token(
                                            leading: SyntaxFactory.TriviaList(SyntaxFactory.Whitespace("        ")),
                                            kind: SyntaxKind.OpenBraceToken,
                                            trailing: SyntaxFactory.TriviaList(SyntaxFactory.LineFeed)),
                                        accessors: SyntaxFactory.List(
                                            new[]
                                            {
                                                SyntaxFactory.AccessorDeclaration(
                                                    kind: SyntaxKind.GetAccessorDeclaration,
                                                    attributeLists: property.Getter()!.AttributeLists,
                                                    modifiers: property.Getter()!.Modifiers,
                                                    keyword: SyntaxFactory.Token(
                                                        leading: SyntaxFactory.TriviaList(SyntaxFactory.Whitespace("            ")),
                                                        kind: SyntaxKind.GetKeyword,
                                                        trailing: SyntaxFactory.TriviaList(SyntaxFactory.Space)),
                                                    body: default,
                                                    expressionBody: SyntaxFactory.ArrowExpressionClause(
                                                        expression: SyntaxFactory.CastExpression(
                                                            type: property.Type,
                                                            expression: SyntaxFactory.InvocationExpression(
                                                                expression: MethodAccess("GetValue"),
                                                                argumentList: SyntaxFactory.ArgumentList(
                                                                    arguments: SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                                                        SyntaxFactory.Argument(
                                                                            expression: SyntaxFactory.IdentifierName(field))))))),
                                                    semicolonToken: SyntaxFactory.Token(
                                                        leading: default,
                                                        kind: SyntaxKind.SemicolonToken,
                                                        trailing: SyntaxFactory.TriviaList(SyntaxFactory.LineFeed))),
                                                SyntaxFactory.AccessorDeclaration(
                                                    kind: SyntaxKind.SetAccessorDeclaration,
                                                    attributeLists: property.Setter()!.AttributeLists,
                                                    modifiers: property.Setter()!.Modifiers,
                                                    keyword: SyntaxFactory.Token(
                                                        leading: SyntaxFactory.TriviaList(SyntaxFactory.Whitespace("            ")),
                                                        kind: SyntaxKind.SetKeyword,
                                                        trailing: SyntaxFactory.TriviaList(SyntaxFactory.Space)),
                                                    body: default,
                                                    expressionBody: SyntaxFactory.ArrowExpressionClause(
                                                        expression: SyntaxFactory.InvocationExpression(
                                                            expression: MethodAccess("SetValue"),
                                                            argumentList: SyntaxFactory.ArgumentList(
                                                                arguments: SyntaxFactory.SeparatedList(
                                                                    new []
                                                                    {
                                                                        SyntaxFactory.Argument(
                                                                            expression: SyntaxFactory.IdentifierName(field)),
                                                                        SyntaxFactory.Argument(
                                                                            expression: SyntaxFactory.IdentifierName("value")),
                                                                    })))),
                                                    semicolonToken: SyntaxFactory.Token(
                                                        leading: default,
                                                        kind: SyntaxKind.SemicolonToken,
                                                        trailing: SyntaxFactory.TriviaList(SyntaxFactory.LineFeed))),
                                            }),
                                        closeBraceToken: SyntaxFactory.Token(
                                            leading: SyntaxFactory.TriviaList(SyntaxFactory.Whitespace("        ")),
                                            kind: SyntaxKind.CloseBraceToken,
                                            trailing: SyntaxFactory.TriviaList(SyntaxFactory.LineFeed))));

                                ExpressionSyntax MethodAccess(string name)
                                {
                                    return qualifyMethodAccess
                                        ? (ExpressionSyntax)SyntaxFactory.MemberAccessExpression(
                                            kind: SyntaxKind.SimpleMemberAccessExpression,
                                            expression: SyntaxFactory.ThisExpression(),
                                            name: SyntaxFactory.IdentifierName(name))
                                        : SyntaxFactory.IdentifierName(name);
                                }
                            }
                        }

                        if (property.Setter() is { } setter &&
                            setter.Modifiers.Any())
                        {
                            //context.RegisterRefactoring(
                            //    CodeAction.Create(
                            //        "Change to readonly dependency property",
                            //        _ => Replace(classDeclaration, classDeclaration, semanticModel),
                            //        "Change to readonly dependency property"));
                        }
                    }
                }

                FieldDeclarationSyntax Field(QualifiedType type, PropertyDeclarationSyntax property, ExpressionSyntax value, SemanticModel semanticModel)
                {
                    return SyntaxFactory.FieldDeclaration(
                        attributeLists: default,
                        modifiers: SyntaxFactory.TokenList(
                            Public(),
                            SyntaxFactory.Token(SyntaxKind.StaticKeyword),
                            SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)),
                        declaration: SyntaxFactory.VariableDeclaration(
                            type: SyntaxFactory.IdentifierName(type.Type),
                            variables: SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.VariableDeclarator(
                                    identifier: SyntaxFactory.Identifier(property.Identifier + "Property"),
                                    argumentList: default,
                                    initializer: SyntaxFactory.EqualsValueClause(
                                        value: value)))),
                        semicolonToken: SyntaxFactory.Token(SyntaxKind.SemicolonToken));

                    SyntaxToken Public()
                    {
                        if (Descriptors.WPF0060DocumentDependencyPropertyBackingMember.IsSuppressed(semanticModel))
                        {
                            return SyntaxFactory.Token(
                                SyntaxFactory.TriviaList(SyntaxFactory.ElasticLineFeed),
                                SyntaxKind.PublicKeyword,
                                SyntaxFactory.TriviaList(SyntaxFactory.Space));
                        }

                        return SyntaxFactory.Token(
                            leading: SyntaxFactory.TriviaList(
                                SyntaxFactory.ElasticLineFeed,
                                SyntaxFactory.Whitespace("        "),
                                SyntaxFactory.Trivia(
                                    SyntaxFactory.DocumentationCommentTrivia(
                                        kind: SyntaxKind.SingleLineDocumentationCommentTrivia,
                                        content: SyntaxFactory.List(
                                            new XmlNodeSyntax[]
                                            {
                                                SyntaxFactory.XmlText(
                                                    textTokens: SyntaxFactory.TokenList(
                                                        SyntaxFactory.XmlTextLiteral(
                                                            leading: SyntaxFactory.TriviaList(
                                                                SyntaxFactory.DocumentationCommentExterior("///")),
                                                            text: " ",
                                                            value: " ",
                                                            trailing: default))),
                                                SyntaxFactory.XmlElement(
                                                    startTag: SyntaxFactory.XmlElementStartTag(
                                                        name: SyntaxFactory.XmlName(localName: SyntaxFactory.Identifier(text: "summary"))),
                                                    content: SyntaxFactory.List(
                                                        new XmlNodeSyntax[]
                                                        {
                                                            SyntaxFactory.XmlText(
                                                                textTokens: SyntaxFactory.TokenList(
                                                                    SyntaxFactory.XmlTextLiteral("Identifies the "))),
                                                            SyntaxFactory.XmlEmptyElement(
                                                                lessThanToken: SyntaxFactory.Token(SyntaxKind.LessThanToken),
                                                                name: SyntaxFactory.XmlName(
                                                                    prefix: default,
                                                                    localName: SyntaxFactory.Identifier(
                                                                        leading: default,
                                                                        text: "see",
                                                                        trailing: default)),
                                                                attributes: SyntaxFactory.SingletonList<XmlAttributeSyntax>(
                                                                    SyntaxFactory.XmlCrefAttribute(
                                                                        name: SyntaxFactory.XmlName(
                                                                            prefix: default,
                                                                            localName: SyntaxFactory.Identifier(
                                                                                leading: SyntaxFactory.TriviaList(SyntaxFactory.Space),
                                                                                text: "cref",
                                                                                trailing: default)),
                                                                        equalsToken: SyntaxFactory.Token(SyntaxKind.EqualsToken),
                                                                        startQuoteToken: SyntaxFactory.Token(SyntaxKind.DoubleQuoteToken),
                                                                        cref: SyntaxFactory.NameMemberCref(
                                                                            name: SyntaxFactory.IdentifierName(
                                                                                identifier: property.Identifier.WithoutTrivia()),
                                                                            parameters: default),
                                                                        endQuoteToken: SyntaxFactory.Token(SyntaxKind.DoubleQuoteToken))),
                                                                slashGreaterThanToken: SyntaxFactory.Token(SyntaxKind.SlashGreaterThanToken)),
                                                            SyntaxFactory.XmlText(
                                                                textTokens: SyntaxFactory.TokenList(
                                                                    SyntaxFactory.XmlTextLiteral(" dependency property."))),
                                                        }),
                                                    endTag: SyntaxFactory.XmlElementEndTag(
                                                        lessThanSlashToken: SyntaxFactory.Token(SyntaxKind.LessThanSlashToken),
                                                        name: SyntaxFactory.XmlName(localName: SyntaxFactory.Identifier(text: "summary")),
                                                        greaterThanToken: SyntaxFactory.Token(SyntaxKind.GreaterThanToken))),
                                                SyntaxFactory.XmlText(
                                                    textTokens: SyntaxFactory.TokenList(
                                                        SyntaxFactory.XmlTextNewLine(
                                                            leading: default,
                                                            text: "\r\n",
                                                            value: "\r\n",
                                                            trailing: default))),
                                            }),
                                        endOfComment: SyntaxFactory.Token(SyntaxKind.EndOfDocumentationCommentToken))),
                                SyntaxFactory.Whitespace("        ")),
                            kind: SyntaxKind.PublicKeyword,
                            trailing: SyntaxFactory.TriviaList(SyntaxFactory.Space));
                    }
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
                                                        type: SyntaxFactory.IdentifierName(containingClass.Identifier),
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
    }
}
