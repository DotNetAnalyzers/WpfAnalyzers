namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FixCastCodeFixProvider))]
    [Shared]
    internal class FixCastCodeFixProvider : DocumentEditorCodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.WPF0019CastSenderToCorrectType.Id,
            Descriptors.WPF0020CastValueToCorrectType.Id,
            Descriptors.WPF0021DirectCastSenderToExactType.Id,
            Descriptors.WPF0022DirectCastValueToExactType.Id);

        /// <inheritdoc/>
        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var document = context.Document;
            var syntaxRoot = await document.GetSyntaxRootAsync(context.CancellationToken)
                                           .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNode(diagnostic, out TypeSyntax? typeSyntax) &&
                    diagnostic.Properties.TryGetValue("ExpectedType", out var registeredType))
                {
                    context.RegisterCodeFix(
                        $"Change type to: {registeredType}.",
                        (e, _) => e.ReplaceNode(
                            typeSyntax,
                            x => SyntaxFactory.ParseTypeName(registeredType).WithTriviaFrom(x)),
                        this.GetType().FullName,
                        diagnostic);
                }
            }
        }
    }
}
