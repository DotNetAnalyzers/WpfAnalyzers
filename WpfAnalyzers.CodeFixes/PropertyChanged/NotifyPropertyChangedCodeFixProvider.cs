namespace WpfAnalyzers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;
    using WpfAnalyzers.PropertyChanged;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NotifyPropertyChangedCodeFixProvider))]
    [Shared]
    internal class NotifyPropertyChangedCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(WPF1012NotifyWhenPropertyChanges.DiagnosticId);

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider() => BacthFixer.Default;

        /// <inheritdoc/>
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);

            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                             .ConfigureAwait(false);
            var syntaxGenerator = SyntaxGenerator.GetGenerator(context.Document);
            var usesUnderscoreNames = syntaxRoot.UsesUnderscoreNames();

            foreach (var diagnostic in context.Diagnostics)
            {
                var fix = CreateFix(diagnostic, syntaxRoot, semanticModel, context.CancellationToken, syntaxGenerator, usesUnderscoreNames);
                if (fix.OnPropertyChanged != null)
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            "Notify property change.",
                            _ => Task.FromResult(context.Document.WithSyntaxRoot(ApplyFix(syntaxRoot, fix, syntaxGenerator))),
                            this.GetType().Name),
                        diagnostic);
                }
            }
        }

        private static Fix CreateFix(Diagnostic diagnostic, SyntaxNode syntaxRoot, SemanticModel semanticModel, CancellationToken cancellationToken, SyntaxGenerator syntaxGenerator, bool usesUnderscoreNames)
        {
            var token = syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start);
            if (string.IsNullOrEmpty(token.ValueText))
            {
                return default(Fix);
            }

            var assignment = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                       .FirstAncestorOrSelf<ExpressionSyntax>();
            var typeDeclaration = assignment?.FirstAncestorOrSelf<TypeDeclarationSyntax>();
            if (typeDeclaration == null)
            {
                return default(Fix);
            }

            string property;
            if (!diagnostic.Properties.TryGetValue(WPF1012NotifyWhenPropertyChanges.PropertyNameKey, out property))
            {
                return default(Fix);
            }

            var type = semanticModel.GetDeclaredSymbolSafe(typeDeclaration, cancellationToken);

            IMethodSymbol invoker;
            if (PropertyChanged.Helpers.PropertyChanged.TryGetInvoker(type, semanticModel, cancellationToken, out invoker) &&
                invoker.Parameters[0].Type == KnownSymbol.String)
            {
                var onPropertyChanged = syntaxGenerator.OnPropertyChanged(property, false, usesUnderscoreNames, invoker);
                return new Fix(assignment, onPropertyChanged, invoker);
            }

            return default(Fix);
        }

        private static SyntaxNode ApplyFix(SyntaxNode syntaxRoot, Fix fix, SyntaxGenerator syntaxGenerator)
        {
            var assignment = syntaxRoot.GetCurrentNode(fix.Assignment);
            if (assignment == null)
            {
                assignment = fix.Assignment;
            }

            var assignStatement = assignment.FirstAncestorOrSelf<ExpressionStatementSyntax>();
            var anonymousFunction = assignment.Parent as AnonymousFunctionExpressionSyntax;
            if (anonymousFunction != null)
            {
                var block = anonymousFunction.Body as BlockSyntax;
                if (block != null)
                {
                    var previousStatement = InsertAfter(block, assignStatement, fix.Invoker);
                    return syntaxRoot.InsertNodesAfter(previousStatement, new[] { fix.OnPropertyChanged });
                }

                var expressionStatement = (ExpressionStatementSyntax)syntaxGenerator.ExpressionStatement(anonymousFunction.Body);
                var withStatements = syntaxGenerator.WithStatements(anonymousFunction, new[] { expressionStatement, fix.OnPropertyChanged });
                return syntaxRoot.ReplaceNode(anonymousFunction, withStatements);
            }
            else
            {
                var block = assignStatement?.Parent as BlockSyntax;
                if (block == null)
                {
                    return syntaxRoot;
                }

                var previousStatement = InsertAfter(block, assignStatement, fix.Invoker);
                return syntaxRoot.InsertNodesAfter(previousStatement, new[] { fix.OnPropertyChanged });
            }
        }

        private static StatementSyntax InsertAfter(BlockSyntax block, ExpressionStatementSyntax assignStatement, IMethodSymbol invoker)
        {
            var index = block.Statements.IndexOf(assignStatement);
            StatementSyntax previousStatement = assignStatement;
            for (var i = index + 1; i < block.Statements.Count; i++)
            {
                var statement = block.Statements[i] as ExpressionStatementSyntax;
                var invocation = statement?.Expression as InvocationExpressionSyntax;
                if (invocation == null)
                {
                    break;
                }

                var identifierName = invocation.Expression as IdentifierNameSyntax;
                if (identifierName == null)
                {
                    var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
                    if (!(memberAccess?.Expression is ThisExpressionSyntax))
                    {
                        break;
                    }

                    identifierName = memberAccess.Name as IdentifierNameSyntax;
                }

                if (identifierName == null)
                {
                    break;
                }

                if (identifierName.Identifier.ValueText == invoker.Name)
                {
                    previousStatement = statement;
                }
            }

            return previousStatement;
        }

        private struct Fix
        {
            internal readonly ExpressionSyntax Assignment;
            internal readonly StatementSyntax OnPropertyChanged;
            internal readonly IMethodSymbol Invoker;

            public Fix(ExpressionSyntax assignment, StatementSyntax onPropertyChanged, IMethodSymbol invoker)
            {
                this.Assignment = assignment;
                this.OnPropertyChanged = onPropertyChanged;
                this.Invoker = invoker;
            }
        }

        private class BacthFixer : FixAllProvider
        {
            public static readonly BacthFixer Default = new BacthFixer();
            private static readonly ImmutableArray<FixAllScope> SupportedFixAllScopes = ImmutableArray.Create(FixAllScope.Document);

            private BacthFixer()
            {
            }

            public override IEnumerable<FixAllScope> GetSupportedFixAllScopes()
            {
                return SupportedFixAllScopes;
            }

            [SuppressMessage("ReSharper", "RedundantCaseLabel")]
            public override Task<CodeAction> GetFixAsync(FixAllContext fixAllContext)
            {
                switch (fixAllContext.Scope)
                {
                    case FixAllScope.Document:
                        return Task.FromResult(CodeAction.Create(
                            "Notify property change.",
                            _ => FixDocumentAsync(fixAllContext),
                            this.GetType().Name));
                    case FixAllScope.Project:
                    case FixAllScope.Solution:
                    case FixAllScope.Custom:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            private static async Task<Document> FixDocumentAsync(FixAllContext context)
            {
                var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                              .ConfigureAwait(false);
                var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                                 .ConfigureAwait(false);
                var syntaxGenerator = SyntaxGenerator.GetGenerator(context.Document);
                var usesUnderscoreNames = syntaxRoot.UsesUnderscoreNames();

                var diagnostics = await context.GetDocumentDiagnosticsAsync(context.Document).ConfigureAwait(false);
                var fixes = new List<Fix>();
                foreach (var diagnostic in diagnostics)
                {
                    var fix = CreateFix(diagnostic, syntaxRoot, semanticModel, context.CancellationToken, syntaxGenerator, usesUnderscoreNames);
                    if (fix.OnPropertyChanged != null)
                    {
                        fixes.Add(fix);
                    }
                }

                if (fixes.Count == 0)
                {
                    return context.Document;
                }

                if (fixes.Count == 1)
                {
                    return context.Document.WithSyntaxRoot(ApplyFix(syntaxRoot, fixes[0], syntaxGenerator));
                }

                var tracking = syntaxRoot.TrackNodes(fixes.Select(x => x.Assignment));
                foreach (var fix in fixes)
                {
                    tracking = ApplyFix(tracking, fix, syntaxGenerator);
                }

                return context.Document.WithSyntaxRoot(tracking);
            }
        }
    }
}