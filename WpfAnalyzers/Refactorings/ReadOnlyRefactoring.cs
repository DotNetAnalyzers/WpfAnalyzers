namespace WpfAnalyzers.Refactorings;

using System;
using System.Composition;
using System.Threading.Tasks;

using Gu.Roslyn.AnalyzerExtensions;
using Gu.Roslyn.CodeFixExtensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(ReadOnlyRefactoring))]
[Shared]
internal class ReadOnlyRefactoring : CodeRefactoringProvider
{
    public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
    {
        var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                      .ConfigureAwait(false);
        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                         .ConfigureAwait(false);
        if (syntaxRoot?.FindNode(context.Span) is { } node &&
            node.FirstAncestorOrSelf<FieldDeclarationSyntax>() is { Declaration: { Type: { } type, Variables: { Count: 1 } variables }, Parent: ClassDeclarationSyntax containingClass } &&
            variables[0] is { Initializer.Value: InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Name: IdentifierNameSyntax methodName } } register } variable &&
            semanticModel is { })
        {
            if (type == KnownSymbols.DependencyProperty)
            {
                context.RegisterRefactoring(
                    CodeAction.Create(
                        "Change to readonly dependency property",
                        _ => WithAttachedProperty(),
                        "Change to readonly dependency property"));

                Task<Document> WithAttachedProperty()
                {
                    var updatedClass = Replace.Usages(
                        containingClass
                            .ReplaceNode(
                                register,
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.IdentifierName(variable!.Identifier.Text + "Key"),
                                    SyntaxFactory.IdentifierName("DependencyProperty")))
                            .AddField(
                                Field(
                                    KnownSymbols.DependencyPropertyKey,
                                    variable.Identifier.Text + "Key",
                                    register.ReplaceNode(methodName, methodName.WithIdentifier(SyntaxFactory.Identifier(methodName.Identifier.Text + "ReadOnly"))))),
                        variable.Identifier.ValueText,
                        variable.Identifier.Text + "Key");
                    return Task.FromResult(
                        context.Document.WithSyntaxRoot(
                            syntaxRoot!.ReplaceNode(containingClass, updatedClass)!));
                }
            }

            static FieldDeclarationSyntax Field(QualifiedType type, string name, ExpressionSyntax value)
            {
                return SyntaxFactory.FieldDeclaration(
                    attributeLists: default,
                    modifiers: SyntaxFactory.TokenList(
                        SyntaxFactory.Token(default, PublicOrPrivate(), SyntaxFactory.TriviaList(SyntaxFactory.Space)),
                        SyntaxFactory.Token(SyntaxKind.StaticKeyword),
                        SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)),
                    declaration: SyntaxFactory.VariableDeclaration(
                        type: SyntaxFactory.IdentifierName(type.Type),
                        variables: SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.VariableDeclarator(
                                identifier: SyntaxFactory.Identifier(name),
                                argumentList: default,
                                initializer: SyntaxFactory.EqualsValueClause(
                                    value: value)))),
                    semicolonToken: SyntaxFactory.Token(default, SyntaxKind.SemicolonToken, default));

                SyntaxKind PublicOrPrivate()
                {
                    return type switch
                    {
                        { Type: "DependencyProperty" } _ => SyntaxKind.PublicKeyword,
                        { Type: "DependencyPropertyKey" } _ => SyntaxKind.PrivateKeyword,
                        _ => throw new InvalidOperationException(),
                    };
                }
            }
        }
    }

    private class Replace : CSharpSyntaxRewriter
    {
        private readonly string before;
        private readonly string after;

        private Replace(string before, string after)
        {
            this.before = before;
            this.after = after;
        }

        public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
        {
            return node switch
            {
                { Parent: ArgumentSyntax { Parent: ArgumentListSyntax { Parent: InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Name.Identifier.ValueText: "SetValue" } } } } }
                    when node.Identifier.Text == this.before
                    => node.WithIdentifier(SyntaxFactory.Identifier(this.after))!,
                _ => base.VisitIdentifierName(node)!,
            };
        }

        internal static ClassDeclarationSyntax Usages(ClassDeclarationSyntax classDeclaration, string before, string after)
        {
            return (ClassDeclarationSyntax)new Replace(before, after).Visit(classDeclaration);
        }
    }
}
