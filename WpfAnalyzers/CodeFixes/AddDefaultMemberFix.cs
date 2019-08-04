namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;
    using Microsoft.CodeAnalysis.Formatting;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddDefaultMemberFix))]
    [Shared]
    internal class AddDefaultMemberFix : DocumentEditorCodeFixProvider
    {
        private const string DefaultFieldFormat = "{0} static readonly {1} Default = new {1}();";
        private const string DefaultDocs = "/// <summary> Gets the default instance </summary>";
        private const string DefaulPropertyFormat = "{0} static {1} Default {{ get; }} = new {1}();";

        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            WPF0070ConverterDoesNotHaveDefaultField.Descriptor.Id);

        /// <inheritdoc/>
        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
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
                    context.RegisterCodeFix(
                        "Add default field.",
                        (e, _) => AddDefaultField(e, classDeclarationSyntax),
                        "Add default field.",
                        diagnostic);

                    context.RegisterCodeFix(
                        "Add default field with docs.",
                        (e, _) => AddDefaultFieldWithDocs(e, classDeclarationSyntax),
                        "Add default field with docs.",
                        diagnostic);

                    context.RegisterCodeFix(
                        "Add default property.",
                        (e, _) => AddDefaultProperty(e, classDeclarationSyntax),
                        "Add default property.",
                        diagnostic);

                    context.RegisterCodeFix(
                        "Add default property with docs.",
                        (e, _) => AddDefaultPropertyWithDocs(e, classDeclarationSyntax),
                        "Add default property with docs.",
                        diagnostic);
                }
            }
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private static DocumentEditor AddDefaultField(DocumentEditor editor, ClassDeclarationSyntax containingType)
        {
            return editor.AddField(containingType, ParseField(string.Format(DefaultFieldFormat, Modifier(containingType), containingType.Identifier.ValueText)))
                         .MakeSealed(containingType);
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private static DocumentEditor AddDefaultFieldWithDocs(DocumentEditor editor, ClassDeclarationSyntax containingType)
        {
            var code = StringBuilderPool.Borrow()
                                        .AppendLine(DefaultDocs)
                                        .AppendLine(string.Format(DefaultFieldFormat, Modifier(containingType), containingType.Identifier.ValueText))
                                        .Return();
            return editor.AddField(containingType, ParseField(code))
                         .MakeSealed(containingType);
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private static DocumentEditor AddDefaultProperty(DocumentEditor editor, ClassDeclarationSyntax containingType)
        {
            return editor.AddProperty(containingType, ParseProperty(string.Format(DefaulPropertyFormat, Modifier(containingType), containingType.Identifier.ValueText)))
                         .MakeSealed(containingType);
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private static DocumentEditor AddDefaultPropertyWithDocs(DocumentEditor editor, ClassDeclarationSyntax containingType)
        {
            var code = StringBuilderPool.Borrow()
                                        .AppendLine(DefaultDocs)
                                        .AppendLine(string.Format(DefaulPropertyFormat, Modifier(containingType), containingType.Identifier.ValueText))
                                        .Return();
            return editor.AddProperty(containingType, ParseProperty(code))
                         .MakeSealed(containingType);
        }

        private static FieldDeclarationSyntax ParseField(string code)
        {
            return Parse.FieldDeclaration(code)
                        .WithSimplifiedNames()
                        .WithLeadingElasticLineFeed()
                        .WithTrailingElasticLineFeed()
                        .WithAdditionalAnnotations(Formatter.Annotation);
        }

        private static PropertyDeclarationSyntax ParseProperty(string code)
        {
            return Parse.PropertyDeclaration(code)
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
