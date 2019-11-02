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
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.WPF0120RegisterContainingMemberAsNameForRoutedCommand.Id,
            Descriptors.WPF0150UseNameofInsteadOfLiteral.Id,
            Descriptors.WPF0151UseNameofInsteadOfConstant.Id);

        /// <inheritdoc/>
        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNode(diagnostic, out ExpressionSyntax? expression) &&
                    diagnostic.Properties.TryGetValue(nameof(IdentifierNameSyntax), out var name))
                {
                    context.RegisterCodeFix(
                        $"Use nameof({name})",
                        (editor, cancellationToken) => FixAsync(editor, expression, name, cancellationToken),
                        this.GetType().FullName,
                        diagnostic);
                }
            }
        }

        private static async Task FixAsync(DocumentEditor editor, ExpressionSyntax argument, string name, CancellationToken cancellationToken)
        {
            if (SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None)
            {
                name = "@" + name;
            }

            if (!argument.IsInStaticContext() &&
                editor.SemanticModel.LookupSymbols(argument.SpanStart, name: name).TrySingle(out var member) &&
                (member is IFieldSymbol || member is IPropertySymbol || member is IMethodSymbol) &&
                !member.IsStatic &&
                await Qualify(member).ConfigureAwait(false) != CodeStyleResult.No)
            {
                editor.ReplaceNode(
                    argument,
                    (x, _) => SyntaxFactory.ParseExpression($"nameof(this.{name})").WithTriviaFrom(x));
            }
            else
            {
                editor.ReplaceNode(
                    argument,
                    (x, _) => SyntaxFactory.ParseExpression($"nameof({name})").WithTriviaFrom(x));
            }

            Task<CodeStyleResult> Qualify(ISymbol symbol)
            {
                switch (symbol.Kind)
                {
                    case SymbolKind.Field:
                        return editor.QualifyFieldAccessAsync(cancellationToken);
                    case SymbolKind.Event:
                        return editor.QualifyEventAccessAsync(cancellationToken);
                    case SymbolKind.Property:
                        return editor.QualifyPropertyAccessAsync(cancellationToken);
                    case SymbolKind.Method:
                        return editor.QualifyMethodAccessAsync(cancellationToken);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(symbol));
                }
            }
        }
    }
}
