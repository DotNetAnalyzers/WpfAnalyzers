namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Threading.Tasks;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MoveFix))]
    [Shared]
    internal class MoveFix : DocumentEditorCodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.WPF0031FieldOrder.Id);

        /// <inheritdoc/>
        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNodeOrAncestor(diagnostic, out MemberDeclarationSyntax member) &&
                    diagnostic.AdditionalLocations.TrySingle(out var additionalLocation) &&
                    syntaxRoot.TryFindNodeOrAncestor(additionalLocation, out MemberDeclarationSyntax other) &&
                    member.SharesAncestor(other, out ClassDeclarationSyntax type))
                {
                    context.RegisterCodeFix(
                        $"Move",
                        (e, _) => e.ReplaceNode(
                            type,
                            x =>
                            {
                                if (x.Members.IndexOf(m => m.IsEquivalentTo(member)) is var fromIndex &&
                                    fromIndex >= 0 &&
                                    x.Members.IndexOf(m => m.IsEquivalentTo(other)) is var toIndex &&
                                    toIndex < fromIndex)
                                {
                                    return x.WithMembers(x.Members.Replace(x.Members[toIndex], Other())
                                                                  .RemoveAt(fromIndex)
                                                                  .Insert(toIndex, Member()));

                                    MemberDeclarationSyntax Member()
                                    {
                                        if (fromIndex == 0)
                                        {
                                            return member.WithLeadingLineFeed();
                                        }

                                        if (toIndex == 0)
                                        {
                                            return member.WithLeadingTrivia(member.GetLeadingTrivia().SkipWhile(t => t.IsKind(SyntaxKind.EndOfLineTrivia)));
                                        }

                                        return member;
                                    }

                                    MemberDeclarationSyntax Other()
                                    {
                                        if (toIndex == 0)
                                        {
                                            return other.WithLeadingLineFeed();
                                        }

                                        return other;
                                    }
                                }

                                return x;
                            }),
                        nameof(MoveFix),
                        diagnostic);
                }
            }
        }
    }
}
