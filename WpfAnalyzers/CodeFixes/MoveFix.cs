namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;

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
                if (syntaxRoot.TryFindNodeOrAncestor(diagnostic, out MemberDeclarationSyntax? toMove) &&
                    diagnostic.AdditionalLocations.TrySingle(out var additionalLocation) &&
                    syntaxRoot.TryFindNodeOrAncestor(additionalLocation, out MemberDeclarationSyntax? member))
                {
                    context.RegisterCodeFix(
                        $"Move",
                        (e, _) => Move(e),
                        nameof(MoveFix),
                        diagnostic);

                    void Move(DocumentEditor editor)
                    {
                        editor.RemoveNode(toMove);
                        editor.InsertBefore(member, ToMove());
                        editor.ReplaceNode(member, Member());

                        MemberDeclarationSyntax ToMove()
                        {
                            if (toMove.Parent is TypeDeclarationSyntax type)
                            {
                                if (type.Members.IndexOf(toMove) == 0)
                                {
                                    return toMove.WithLeadingLineFeed();
                                }

                                if (type.Members.IndexOf(member) == 0)
                                {
                                    return toMove.WithoutLeadingLineFeed();
                                }
                            }

                            return member;
                        }

                        MemberDeclarationSyntax Member()
                        {
                            if (member.Parent is TypeDeclarationSyntax type &&
                                type.Members.IndexOf(member) == 0)
                            {
                                return member.WithLeadingLineFeed();
                            }

                            return member;
                        }
                    }
                }
            }
        }
    }
}
