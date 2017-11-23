namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;
    using Microsoft.CodeAnalysis.Formatting;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddDefaultMemberFix))]
    [Shared]
    internal class AddDefaultMemberFix : CodeFixProvider
    {
        private const string DefaultFieldFormat = "{0} static readonly {1} Default = new {1}();";
        private const string DefaultDocs = "/// <summary> Gets the default instance </summary>";
        private const string DefaulPropertyFormat = "{0} static {1} Default {{ get; }} = new {1}();";

        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(WPF0070ConverterDoesNotHaveDefaultField.DiagnosticId);

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        /// <inheritdoc/>
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var syntaxRoot = await document.GetSyntaxRootAsync(context.CancellationToken)
                                           .ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                var token = syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start);
                if (string.IsNullOrEmpty(token.ValueText))
                {
                    continue;
                }

                var classDeclarationSyntax = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                         .FirstAncestorOrSelf<ClassDeclarationSyntax>();
                if (classDeclarationSyntax != null)
                {
                    context.RegisterDocumentEditorFix(
                        $"Add default field.",
                        (e, _) => AddDefaultField(e, classDeclarationSyntax),
                        diagnostic);

                    context.RegisterDocumentEditorFix(
                        $"Add default field with docs.",
                        (e, _) => AddDefaultFieldWithDocs(e, classDeclarationSyntax),
                        diagnostic);

                    context.RegisterDocumentEditorFix(
                        $"Add default property.",
                        (e, _) => AddDefaultProperty(e, classDeclarationSyntax),
                        diagnostic);

                    context.RegisterDocumentEditorFix(
                        $"Add default property with docs.",
                        (e, _) => AddDefaultPropertyWithDocs(e, classDeclarationSyntax),
                        diagnostic);
                }
            }
        }

        private static void AddDefaultField(DocumentEditor editor, ClassDeclarationSyntax containingType)
        {
            editor.AddField(containingType, (FieldDeclarationSyntax)ParseMember(string.Format(DefaultFieldFormat, Modifier(containingType), containingType.Identifier.ValueText)));
            editor.MakeSealed(containingType);
        }

        private static void AddDefaultFieldWithDocs(DocumentEditor editor, ClassDeclarationSyntax containingType)
        {
            var code = StringBuilderPool.Borrow()
                                        .AppendLine(DefaultDocs)
                                        .AppendLine(string.Format(DefaultFieldFormat, Modifier(containingType), containingType.Identifier.ValueText))
                                        .Return();
            editor.AddField(containingType, (FieldDeclarationSyntax)ParseMember(code));
            editor.MakeSealed(containingType);
        }

        private static void AddDefaultProperty(DocumentEditor editor, ClassDeclarationSyntax containingType)
        {
            editor.AddProperty(containingType, (PropertyDeclarationSyntax)ParseMember(string.Format(DefaulPropertyFormat, Modifier(containingType), containingType.Identifier.ValueText)));
            editor.MakeSealed(containingType);
        }

        private static void AddDefaultPropertyWithDocs(DocumentEditor editor, ClassDeclarationSyntax containingType)
        {
            var code = StringBuilderPool.Borrow()
                                        .AppendLine(DefaultDocs)
                                        .AppendLine(string.Format(DefaulPropertyFormat, Modifier(containingType), containingType.Identifier.ValueText))
                                        .Return();
            editor.AddProperty(containingType, (PropertyDeclarationSyntax)ParseMember(code));
            editor.MakeSealed(containingType);
        }

        private static MemberDeclarationSyntax ParseMember(string code)
        {
            return SyntaxFactory.ParseCompilationUnit(code)
                                .Members
                                .Single()
                                .WithSimplifiedNames()
                                .WithLeadingElasticLineFeed()
                                .WithTrailingElasticLineFeed()
                                .WithAdditionalAnnotations(Formatter.Annotation);
        }

        private static string Modifier(ClassDeclarationSyntax containingType)
        {
            foreach (var modifier in containingType.Modifiers)
            {
                if (modifier.IsKind(SyntaxKind.PublicKeyword))
                {
                    return "public";
                }

                if (modifier.IsKind(SyntaxKind.InternalKeyword))
                {
                    return "internal";
                }
            }

            return string.Empty;
        }
    }
}