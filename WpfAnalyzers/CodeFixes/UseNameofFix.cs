namespace WpfAnalyzers
{
    using System;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseNameofFix))]
    [Shared]
    internal class UseNameofFix : DocumentEditorCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.WPF0120RegisterContainingMemberAsNameForRoutedCommand.Id,
            Descriptors.WPF0150UseNameofInsteadOfLiteral.Id,
            Descriptors.WPF0151UseNameofInsteadOfConstant.Id);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot is { } &&
                    syntaxRoot.TryFindNode(diagnostic, out ExpressionSyntax? expression) &&
                    diagnostic.Properties.TryGetValue(nameof(IdentifierNameSyntax), out var name) &&
                    name is { })
                {
                    context.RegisterCodeFix(
                        $"Use nameof({name})",
                        (editor, cancellationToken) => FixAsync(editor, cancellationToken),
                        this.GetType().FullName,
                        diagnostic);

                    async Task FixAsync(DocumentEditor editor, CancellationToken cancellationToken)
                    {
                        if (SyntaxFacts.GetKeywordKind(name!) != SyntaxKind.None)
                        {
                            name = "@" + name;
                        }

                        if (!expression!.IsInStaticContext() &&
                            editor.SemanticModel.LookupSymbols(expression!.SpanStart, name: name).TrySingle(out var member) &&
                            (member is IFieldSymbol || member is IPropertySymbol || member is IMethodSymbol) &&
                            !member.IsStatic &&
                            await Qualify(member).ConfigureAwait(false) != CodeStyleResult.No)
                        {
                            editor.ReplaceNode(
                                expression,
                                (x, _) => SyntaxFactory.ParseExpression($"nameof(this.{name})").WithTriviaFrom(x));
                        }
                        else
                        {
                            editor.ReplaceNode(
                                expression,
                                (x, _) => SyntaxFactory.ParseExpression($"nameof({name})").WithTriviaFrom(x));
                        }

                        Task<CodeStyleResult> Qualify(ISymbol symbol)
                        {
                            return symbol.Kind switch
                            {
                                SymbolKind.Field => editor.QualifyFieldAccessAsync(cancellationToken),
                                SymbolKind.Event => editor.QualifyEventAccessAsync(cancellationToken),
                                SymbolKind.Property => editor.QualifyPropertyAccessAsync(cancellationToken),
                                SymbolKind.Method => editor.QualifyMethodAccessAsync(cancellationToken),
                                _ => throw new ArgumentOutOfRangeException(nameof(symbol)),
                            };
                        }
                    }
                }
            }
        }
    }
}
