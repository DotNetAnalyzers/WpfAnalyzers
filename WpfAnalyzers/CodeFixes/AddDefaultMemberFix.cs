namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Globalization;
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
        private const string DefaultPropertyFormat = "{0} static {1} Default {{ get; }} = new {1}();";

        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.WPF0070ConverterDoesNotHaveDefaultField.Id);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var document = context.Document;
            var syntaxRoot = await document.GetSyntaxRootAsync(context.CancellationToken)
                                           .ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNodeOrAncestor(diagnostic, out ClassDeclarationSyntax? classDeclaration))
                {
                    context.RegisterCodeFix(
                        "Add default field.",
                        (e, _) => AddDefaultField(e, classDeclaration),
                        "Add default field.",
                        diagnostic);

                    context.RegisterCodeFix(
                        "Add default field with docs.",
                        (e, _) => AddDefaultFieldWithDocs(e, classDeclaration),
                        "Add default field with docs.",
                        diagnostic);

                    context.RegisterCodeFix(
                        "Add default property.",
                        (e, _) => AddDefaultProperty(e, classDeclaration),
                        "Add default property.",
                        diagnostic);

                    context.RegisterCodeFix(
                        "Add default property with docs.",
                        (e, _) => AddDefaultPropertyWithDocs(e, classDeclaration),
                        "Add default property with docs.",
                        diagnostic);
                }
            }
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private static DocumentEditor AddDefaultField(DocumentEditor editor, ClassDeclarationSyntax containingType)
        {
            return editor.AddField(containingType, ParseField(string.Format(CultureInfo.InvariantCulture, DefaultFieldFormat, Modifier(containingType), containingType.Identifier.ValueText)))
                         .Seal(containingType);
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private static DocumentEditor AddDefaultFieldWithDocs(DocumentEditor editor, ClassDeclarationSyntax containingType)
        {
            var code = StringBuilderPool.Borrow()
                                        .AppendLine(DefaultDocs)
                                        .AppendLine(string.Format(CultureInfo.InvariantCulture, DefaultFieldFormat, Modifier(containingType), containingType.Identifier.ValueText))
                                        .Return();
            return editor.AddField(containingType, ParseField(code))
                         .Seal(containingType);
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private static DocumentEditor AddDefaultProperty(DocumentEditor editor, ClassDeclarationSyntax containingType)
        {
            return editor.AddProperty(containingType, ParseProperty(string.Format(CultureInfo.InvariantCulture, DefaultPropertyFormat, Modifier(containingType), containingType.Identifier.ValueText)))
                         .Seal(containingType);
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private static DocumentEditor AddDefaultPropertyWithDocs(DocumentEditor editor, ClassDeclarationSyntax containingType)
        {
            var code = StringBuilderPool.Borrow()
                                        .AppendLine(DefaultDocs)
                                        .AppendLine(string.Format(CultureInfo.InvariantCulture, DefaultPropertyFormat, Modifier(containingType), containingType.Identifier.ValueText))
                                        .Return();
            return editor.AddProperty(containingType, ParseProperty(code))
                         .Seal(containingType);
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
