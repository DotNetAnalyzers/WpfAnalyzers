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
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;

    using WpfAnalyzers.PropertyChanged;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CheckIfDifferentBeforeNotifyFixProvider))]
    [Shared]
    internal class CheckIfDifferentBeforeNotifyFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(WPF1015CheckIfDifferentBeforeNotifying.DiagnosticId);

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

            foreach (var diagnostic in context.Diagnostics)
            {
                var fix = CreateFix(diagnostic, syntaxRoot, semanticModel, context.CancellationToken, syntaxGenerator, context.Document.Project.CompilationOptions.SpecificDiagnosticOptions);
                if (fix.IfReturn != null)
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            "Check that value is different before notifying.",
                            _ => Task.FromResult(context.Document.WithSyntaxRoot(ApplyFix(syntaxRoot, fix, syntaxGenerator))),
                            this.GetType().Name),
                        diagnostic);
                }
            }
        }

        private static Fix CreateFix(
            Diagnostic diagnostic,
            SyntaxNode syntaxRoot,
            SemanticModel semanticModel,
            CancellationToken cancellationToken,
            SyntaxGenerator syntaxGenerator,
            ImmutableDictionary<string, ReportDiagnostic> diagnosticOptions)
        {
            var token = syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start);
            if (string.IsNullOrEmpty(token.ValueText))
            {
                return default(Fix);
            }

            var invocationStatement = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                                .FirstAncestorOrSelf<StatementSyntax>();
            var setter = invocationStatement?.FirstAncestorOrSelf<AccessorDeclarationSyntax>();
            if (setter?.IsKind(SyntaxKind.SetAccessorDeclaration) != true)
            {
                return default(Fix);
            }

            ExpressionStatementSyntax assignment = null;
            foreach (var statement in setter.Body.Statements)
            {
                var expressionStatement = statement as ExpressionStatementSyntax;
                if (expressionStatement == null)
                {
                    return default(Fix);
                }

                var expression = expressionStatement.Expression;
                if (expression is AssignmentExpressionSyntax)
                {
                    if (assignment != null)
                    {
                        return default(Fix);
                    }

                    assignment = expressionStatement;
                    continue;
                }

                if (PropertyChanged.Helpers.PropertyChanged.IsNotifyPropertyChanged(
                    expressionStatement,
                    semanticModel,
                    cancellationToken))
                {
                    continue;
                }

                return default(Fix);
            }

            var property = semanticModel.GetDeclaredSymbolSafe(
                setter.FirstAncestorOrSelf<PropertyDeclarationSyntax>(),
                cancellationToken);
            IFieldSymbol backingField;
            if (PropertyChanged.Helpers.Property.TryGetBackingField(
                property,
                semanticModel,
                cancellationToken,
                out backingField))
            {
                var ifReturn = syntaxGenerator.IfValueEqualsBackingFieldReturn(backingField.Name, property, diagnosticOptions);
                return new Fix(assignment, ifReturn);
            }

            return default(Fix);
        }

        private static SyntaxNode ApplyFix(SyntaxNode syntaxRoot, Fix fix, SyntaxGenerator syntaxGenerator)
        {
            var assignment = syntaxRoot.GetCurrentNode(fix.Assignment) ?? fix.Assignment;
            return syntaxGenerator.InsertNodesBefore(syntaxRoot, assignment, new[] { fix.IfReturn });
        }

        private struct Fix
        {
            internal readonly ExpressionStatementSyntax Assignment;
            internal readonly IfStatementSyntax IfReturn;

            public Fix(ExpressionStatementSyntax assignment, IfStatementSyntax ifReturn)
            {
                this.Assignment = assignment;
                this.IfReturn = ifReturn;
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

            [SuppressMessage("ReSharper", "RedundantCaseLabel", Justification = "Mute R#")]
            public override Task<CodeAction> GetFixAsync(FixAllContext fixAllContext)
            {
                switch (fixAllContext.Scope)
                {
                    case FixAllScope.Document:
                        return Task.FromResult(CodeAction.Create(
                                                   "Check that value is different before notifying.",
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

                var diagnostics = await context.GetDocumentDiagnosticsAsync(context.Document).ConfigureAwait(false);
                var fixes = new List<Fix>();
                foreach (var diagnostic in diagnostics)
                {
                    var fix = CreateFix(diagnostic, syntaxRoot, semanticModel, context.CancellationToken, syntaxGenerator, context.Document.Project.CompilationOptions.SpecificDiagnosticOptions);
                    if (fix.IfReturn != null)
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