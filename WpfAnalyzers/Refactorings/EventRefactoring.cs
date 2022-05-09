namespace WpfAnalyzers.Refactorings;

using System.Composition;
using System.Threading.Tasks;

using Gu.Roslyn.AnalyzerExtensions;
using Gu.Roslyn.CodeFixExtensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(EventRefactoring))]
[Shared]
internal class EventRefactoring : CodeRefactoringProvider
{
    public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
    {
        var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                      .ConfigureAwait(false);
        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                         .ConfigureAwait(false);

        if (syntaxRoot?.FindNode(context.Span) is { } node &&
            node.FirstAncestorOrSelf<EventFieldDeclarationSyntax>() is { Declaration.Variables.Count: 1, Parent: ClassDeclarationSyntax containingClass } eventDeclaration &&
            !eventDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword) &&
            eventDeclaration.Modifiers.Any(SyntaxKind.PublicKeyword, SyntaxKind.InternalKeyword) &&
            semanticModel?.GetType(containingClass, context.CancellationToken) is { } containingType &&
            containingType.IsAssignableTo(KnownSymbols.DependencyObject, semanticModel.Compilation))
        {
            context.RegisterRefactoring(
                CodeAction.Create(
                    "Change to routed event",
                    _ => WithRoutedEvent(),
                    "Change to routed event"));

            Task<Document> WithRoutedEvent()
            {
                var updatedClass = containingClass.ReplaceNode(eventDeclaration, Event(eventDeclaration))
                                                  .AddField(Field(eventDeclaration, containingClass));

                return Task.FromResult(
                    context.Document.WithSyntaxRoot(
                        syntaxRoot.ReplaceNode(containingClass, updatedClass)!));
            }

            static FieldDeclarationSyntax Field(EventFieldDeclarationSyntax eventDeclaration, ClassDeclarationSyntax containingClass)
            {
                return SyntaxFactory.FieldDeclaration(
                    attributeLists: default,
                    modifiers: SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                        SyntaxFactory.Token(SyntaxKind.StaticKeyword),
                        SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)),
                    declaration: SyntaxFactory.VariableDeclaration(
                        type: SyntaxFactory.ParseTypeName("System.Windows.RoutedEvent")
                                           .WithSimplifiedNames(),
                        variables: SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.VariableDeclarator(
                                identifier: SyntaxFactory.Identifier(eventDeclaration.Declaration.Variables[0].Identifier.ValueText + "Event"),
                                argumentList: default,
                                initializer: SyntaxFactory.EqualsValueClause(
                                    value: Register(eventDeclaration, containingClass))))),
                    semicolonToken: SyntaxFactory.Token(default, SyntaxKind.SemicolonToken, default));

                static InvocationExpressionSyntax Nameof(EventFieldDeclarationSyntax eventDeclaration)
                {
                    return SyntaxFactory.InvocationExpression(
                        expression: SyntaxFactory.IdentifierName(
                            SyntaxFactory.Identifier(
                                SyntaxFactory.TriviaList(
                                    SyntaxFactory.Whitespace(eventDeclaration.LeadingWhitespace() + new string(' ', 4))),
                                SyntaxKind.NameOfKeyword,
                                "nameof",
                                "nameof",
                                default)),
                        argumentList: SyntaxFactory.ArgumentList(
                            arguments: SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Argument(
                                    SyntaxFactory.IdentifierName(identifier: eventDeclaration.Declaration.Variables[0]
                                                                                             .Identifier)))));
                }

                static InvocationExpressionSyntax Register(EventFieldDeclarationSyntax eventDeclaration, ClassDeclarationSyntax containingClass)
                {
                    var leadingWhitespace = SyntaxFactory.Whitespace(containingClass.LeadingWhitespace() + new string(' ', 8));
                    return SyntaxFactory.InvocationExpression(
                        expression: SyntaxFactory.MemberAccessExpression(
                            kind: SyntaxKind.SimpleMemberAccessExpression,
                            expression: SyntaxFactory.ParseTypeName("System.Windows.EventManager")
                                                     .WithSimplifiedNames(),
                            name: SyntaxFactory.IdentifierName("RegisterRoutedEvent")),
                        argumentList: SyntaxFactory.ArgumentList(
                            openParenToken: SyntaxFactory.Token(
                                leading: default,
                                kind: SyntaxKind.OpenParenToken,
                                trailing: SyntaxFactory.TriviaList(SyntaxFactory.LineFeed)),
                            arguments: SyntaxFactory.SeparatedList(
                                new[]
                                {
                                    SyntaxFactory.Argument(expression: Nameof(eventDeclaration)
                                                               .WithLeadingTrivia(leadingWhitespace)),
                                    SyntaxFactory.Argument(expression: SyntaxFactory.ParseExpression("RoutingStrategy.Direct")
                                                                                    .WithLeadingTrivia(leadingWhitespace)),
                                    SyntaxFactory.Argument(
                                        nameColon: default,
                                        refKindKeyword: default,
                                        expression: SyntaxFactory.TypeOfExpression(
                                            keyword: SyntaxFactory.Token(
                                                leading: SyntaxFactory.TriviaList(leadingWhitespace),
                                                kind: SyntaxKind.TypeOfKeyword,
                                                trailing: default),
                                            openParenToken: SyntaxFactory.Token(SyntaxKind.OpenParenToken),
                                            type: Type(eventDeclaration),
                                            closeParenToken: SyntaxFactory.Token(SyntaxKind.CloseParenToken))),
                                    SyntaxFactory.Argument(
                                        nameColon: default,
                                        refKindKeyword: default,
                                        expression: SyntaxFactory.TypeOfExpression(
                                            keyword: SyntaxFactory.Token(
                                                leading: SyntaxFactory.TriviaList(leadingWhitespace),
                                                kind: SyntaxKind.TypeOfKeyword,
                                                trailing: default),
                                            openParenToken: SyntaxFactory.Token(SyntaxKind.OpenParenToken),
                                            type: SyntaxFactory.IdentifierName(containingClass.Identifier.WithoutTrivia()),
                                            closeParenToken: SyntaxFactory.Token(SyntaxKind.CloseParenToken))),
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

                    static TypeSyntax Type(EventFieldDeclarationSyntax eventDeclaration) => eventDeclaration.Declaration.Type switch
                    {
                        NullableTypeSyntax { ElementType: { } elementType } => elementType,
                        { } type => type,
                    };
                }
            }

            static EventDeclarationSyntax Event(EventFieldDeclarationSyntax eventDeclaration)
            {
                return SyntaxFactory.EventDeclaration(
                               attributeLists: default,
                               modifiers: eventDeclaration.Modifiers,
                               eventKeyword: SyntaxFactory.Token(kind: SyntaxKind.EventKeyword),
                               type: eventDeclaration.Declaration.Type,
                               explicitInterfaceSpecifier: default,
                               identifier: SyntaxFactory.Identifier(
                                   leading: default,
                                   text: eventDeclaration.Declaration.Variables[0].Identifier.Text,
                                   trailing: SyntaxFactory.TriviaList(SyntaxFactory.LineFeed)),
                               accessorList: SyntaxFactory.AccessorList(
                                   openBraceToken: SyntaxFactory.Token(
                                       leading: SyntaxFactory.TriviaList(SyntaxFactory.Whitespace(eventDeclaration.LeadingWhitespace())),
                                       kind: SyntaxKind.OpenBraceToken,
                                       trailing: SyntaxFactory.TriviaList(SyntaxFactory.LineFeed)),
                                   accessors: SyntaxFactory.List(
                                       new AccessorDeclarationSyntax[]
                                       {
                        SyntaxFactory.AccessorDeclaration(
                            kind: SyntaxKind.AddAccessorDeclaration,
                            attributeLists: default,
                            modifiers: default,
                            keyword: SyntaxFactory.Token(
                                leading: SyntaxFactory.TriviaList(SyntaxFactory.Whitespace(eventDeclaration.LeadingWhitespace() + new string(' ', 4))),
                                kind: SyntaxKind.AddKeyword,
                                trailing: SyntaxFactory.TriviaList(SyntaxFactory.Space)),
                            body: default,
                            expressionBody: SyntaxFactory.ArrowExpressionClause(
                                arrowToken: SyntaxFactory.Token(
                                    leading: default,
                                    kind: SyntaxKind.EqualsGreaterThanToken,
                                    trailing: SyntaxFactory.TriviaList(SyntaxFactory.Space)),
                                expression: SyntaxFactory.InvocationExpression(
                                    expression: SyntaxFactory.MemberAccessExpression(
                                        kind: SyntaxKind.SimpleMemberAccessExpression,
                                        expression: SyntaxFactory.ThisExpression(
                                            token: SyntaxFactory.Token(SyntaxKind.ThisKeyword)),
                                        operatorToken: SyntaxFactory.Token(SyntaxKind.DotToken),
                                        name: SyntaxFactory.IdentifierName("AddHandler")),
                                    argumentList: SyntaxFactory.ArgumentList(
                                        openParenToken: SyntaxFactory.Token(SyntaxKind.OpenParenToken),
                                        arguments: SyntaxFactory.SeparatedList(
                                            new ArgumentSyntax[]
                                            {
                                                SyntaxFactory.Argument(expression: SyntaxFactory.IdentifierName($"{eventDeclaration.Declaration.Variables[0].Identifier.Text}Event")),
                                                SyntaxFactory.Argument(expression: SyntaxFactory.IdentifierName("value")),
                                            },
                                            new SyntaxToken[]
                                            {
                                                SyntaxFactory.Token(
                                                    leading: default,
                                                    kind: SyntaxKind.CommaToken,
                                                    trailing: SyntaxFactory.TriviaList(SyntaxFactory.Space)),
                                            }),
                                        closeParenToken: SyntaxFactory.Token(SyntaxKind.CloseParenToken)))),
                            semicolonToken: SyntaxFactory.Token(
                                leading: default,
                                kind: SyntaxKind.SemicolonToken,
                                trailing: SyntaxFactory.TriviaList(SyntaxFactory.LineFeed))),
                        SyntaxFactory.AccessorDeclaration(
                            kind: SyntaxKind.RemoveAccessorDeclaration,
                            attributeLists: default,
                            modifiers: default,
                            keyword: SyntaxFactory.Token(
                                leading: SyntaxFactory.TriviaList(SyntaxFactory.Whitespace(eventDeclaration.LeadingWhitespace() + new string(' ', 4))),
                                kind: SyntaxKind.RemoveKeyword,
                                trailing: SyntaxFactory.TriviaList(SyntaxFactory.Space)),
                            body: default,
                            expressionBody: SyntaxFactory.ArrowExpressionClause(
                                arrowToken: SyntaxFactory.Token(
                                    leading: default,
                                    kind: SyntaxKind.EqualsGreaterThanToken,
                                    trailing: SyntaxFactory.TriviaList(SyntaxFactory.Space)),
                                expression: SyntaxFactory.InvocationExpression(
                                    expression: SyntaxFactory.MemberAccessExpression(
                                        kind: SyntaxKind.SimpleMemberAccessExpression,
                                        expression: SyntaxFactory.ThisExpression(
                                            token: SyntaxFactory.Token(SyntaxKind.ThisKeyword)),
                                        operatorToken: SyntaxFactory.Token(SyntaxKind.DotToken),
                                        name: SyntaxFactory.IdentifierName("RemoveHandler")),
                                    argumentList: SyntaxFactory.ArgumentList(
                                        openParenToken: SyntaxFactory.Token(SyntaxKind.OpenParenToken),
                                        arguments: SyntaxFactory.SeparatedList(
                                            new ArgumentSyntax[]
                                            {
                                                SyntaxFactory.Argument(expression: SyntaxFactory.IdentifierName($"{eventDeclaration.Declaration.Variables[0].Identifier.Text}Event")),
                                                SyntaxFactory.Argument(expression: SyntaxFactory.IdentifierName("value")),
                                            },
                                            new SyntaxToken[]
                                            {
                                                SyntaxFactory.Token(
                                                    leading: default,
                                                    kind: SyntaxKind.CommaToken,
                                                    trailing: SyntaxFactory.TriviaList(SyntaxFactory.Space)),
                                            }),
                                        closeParenToken: SyntaxFactory.Token(SyntaxKind.CloseParenToken)))),
                            semicolonToken: SyntaxFactory.Token(
                                leading: default,
                                kind: SyntaxKind.SemicolonToken,
                                trailing: SyntaxFactory.TriviaList(SyntaxFactory.LineFeed))),
                                       }),
                                   closeBraceToken: SyntaxFactory.Token(
                                       leading: SyntaxFactory.TriviaList(SyntaxFactory.Whitespace(eventDeclaration.LeadingWhitespace())),
                                       kind: SyntaxKind.CloseBraceToken,
                                       trailing: SyntaxFactory.TriviaList(SyntaxFactory.LineFeed))),
                               semicolonToken: default);
            }
        }
    }
}
