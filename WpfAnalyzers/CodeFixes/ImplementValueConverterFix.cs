// ReSharper disable InconsistentNaming
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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ImplementValueConverterFix))]
    [Shared]
    internal class ImplementValueConverterFix : DocumentEditorCodeFixProvider
    {
        private static readonly MethodDeclarationSyntax IValueConverterConvert = ParseMethod(
            @"        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }");

        private static readonly MethodDeclarationSyntax IMultiValueConverterConvert = ParseMethod(
            @"        public object Convert(object[] values, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }");

        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create("CS0535");

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

                var classDeclaration = syntaxRoot.FindNode(diagnostic.Location.SourceSpan).FirstAncestorOrSelf<ClassDeclarationSyntax>();
                if (classDeclaration == null)
                {
                    continue;
                }

                if (HasInterface(classDeclaration, KnownSymbol.IValueConverter))
                {
                    if (diagnostic.GetMessage(CultureInfo.InvariantCulture)
                                  .Contains("does not implement interface member 'IValueConverter.Convert(object, Type, object, CultureInfo)'"))
                    {
                        context.RegisterCodeFix(
                            "Implement IValueConverter.Convert for one way bindings.",
                            (editor, _) => editor.AddMethod(classDeclaration, IValueConverterConvert),
                            "Implement IValueConverter",
                            diagnostic);
                    }

                    if (diagnostic.GetMessage(CultureInfo.InvariantCulture)
                                  .Contains("does not implement interface member 'IValueConverter.ConvertBack(object, Type, object, CultureInfo)'"))
                    {
                        context.RegisterCodeFix(
                            "Implement IValueConverter.ConvertBack for one way bindings.",
                            (editor, _) => editor.AddMethod(classDeclaration, IValueConverterConvertBack(classDeclaration.Identifier.ValueText)),
                            "Implement IValueConverter",
                            diagnostic);
                    }
                }

                if (HasInterface(classDeclaration, KnownSymbol.IMultiValueConverter))
                {
                    if (diagnostic.GetMessage(CultureInfo.InvariantCulture)
                                  .Contains("does not implement interface member 'IMultiValueConverter.Convert(object[], Type, object, CultureInfo)'"))
                    {
                        context.RegisterCodeFix(
                            "Implement IMultiValueConverter.Convert for one way bindings.",
                            (editor, _) => editor.AddMethod(classDeclaration, IMultiValueConverterConvert),
                            "Implement IMultiValueConverter",
                            diagnostic);
                    }

                    if (diagnostic.GetMessage(CultureInfo.InvariantCulture)
                                  .Contains("does not implement interface member 'IMultiValueConverter.ConvertBack(object, Type[], object, CultureInfo)'"))
                    {
                        context.RegisterCodeFix(
                            "Implement IMultiValueConverter.ConvertBack for one way bindings.",
                            (editor, _) => editor.AddMethod(classDeclaration, IMultiValueConverterConvertBack(classDeclaration.Identifier.ValueText)),
                            "Implement IMultiValueConverter",
                            diagnostic);
                    }
                }
            }
        }

        private static bool HasInterface(ClassDeclarationSyntax classDeclaration, QualifiedType type)
        {
            if (classDeclaration.BaseList == null)
            {
                return false;
            }

            foreach (var typeSyntax in classDeclaration.BaseList.Types)
            {
                if (typeSyntax.Type is SimpleNameSyntax name &&
                    name.Identifier.ValueText == type.Type)
                {
                    return true;
                }

                if (typeSyntax.Type is QualifiedNameSyntax qualifiedName &&
                    qualifiedName.Right is SimpleNameSyntax simpleName &&
                    simpleName.Identifier.ValueText == type.Type)
                {
                    return true;
                }
            }

            return false;
        }

        private static MethodDeclarationSyntax IValueConverterConvertBack(string containingTypeName)
        {
            var code = StringBuilderPool.Borrow()
                                        .AppendLine("        object System.Windows.Data.IValueConverter.ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)")
                                        .AppendLine("        {")
                                        .AppendLine($"            throw new System.NotSupportedException($\"{{nameof({containingTypeName})}} can only be used in OneWay bindings\");")
                                        .AppendLine("        }")
                                        .Return();
            return ParseMethod(code);
        }

        private static MethodDeclarationSyntax IMultiValueConverterConvertBack(string containingTypeName)
        {
            var code = StringBuilderPool.Borrow()
                                        .AppendLine("        object[] System.Windows.Data.IMultiValueConverter.ConvertBack(object value, System.Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)")
                                        .AppendLine("        {")
                                        .AppendLine($"            throw new System.NotSupportedException($\"{{nameof({containingTypeName})}} can only be used in OneWay bindings\");")
                                        .AppendLine("        }")
                                        .Return();
            return ParseMethod(code);
        }

        private static MethodDeclarationSyntax ParseMethod(string code)
        {
            return Parse.MethodDeclaration(code)
                        .WithSimplifiedNames()
                        .WithLeadingTrivia(SyntaxFactory.ElasticMarker)
                        .WithTrailingTrivia(SyntaxFactory.ElasticMarker);
        }
    }
}
