namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;
    using WpfAnalyzers.PropertyChanged;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ImplementINotifyPropertyChangedCodeFixProvider))]
    [Shared]
    internal class ImplementINotifyPropertyChangedCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            WPF1011ImplementINotifyPropertyChanged.DiagnosticId,
            "CS0535");

        /// <inheritdoc/>
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);

            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                             .ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                if (!IsSupportedDiagnostic(diagnostic))
                {
                    continue;
                }

                var typeDeclaration = syntaxRoot.FindNode(diagnostic.Location.SourceSpan).FirstAncestorOrSelf<TypeDeclarationSyntax>();
                if (typeDeclaration == null)
                {
                    continue;
                }

                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Implement IDisposable.",
                        cancellationToken =>
                            ApplyImplementINotifyPropertyChangedFixAsync(
                                context,
                                semanticModel,
                                cancellationToken,
                                (CompilationUnitSyntax)syntaxRoot,
                                typeDeclaration),
                        nameof(ImplementINotifyPropertyChangedCodeFixProvider)),
                    diagnostic);
            }
        }

        private static Task<Document> ApplyImplementINotifyPropertyChangedFixAsync(
            CodeFixContext context,
            SemanticModel semanticModel,
            CancellationToken cancellationToken,
            CompilationUnitSyntax syntaxRoot,
            TypeDeclarationSyntax typeDeclaration)
        {
            var type = semanticModel.GetDeclaredSymbolSafe(typeDeclaration, cancellationToken);
            var syntaxGenerator = SyntaxGenerator.GetGenerator(context.Document);

            var updated = typeDeclaration.WithINotifyPropertyChangedInterface(syntaxGenerator, type)
                                         .WithPropertyChangedEvent(syntaxGenerator)
                                         .WithInvoker(syntaxGenerator, type);
            var newRoot = syntaxRoot.ReplaceNode(typeDeclaration, updated)
                                    .WithUsings();
            return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
        }

        private static bool IsSupportedDiagnostic(Diagnostic diagnostic)
        {
            if (diagnostic.Id == WPF1011ImplementINotifyPropertyChanged.DiagnosticId)
            {
                return true;
            }

            if (diagnostic.Id == "CS0535")
            {
                return diagnostic.GetMessage(CultureInfo.InvariantCulture)
                                 .EndsWith("does not implement interface member 'INotifyPropertyChanged.PropertyChanged'");
            }

            return false;
        }
    }
}