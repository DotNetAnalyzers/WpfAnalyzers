// ReSharper disable InconsistentNaming
namespace WpfAnalyzers;

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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ImplementValueConverterFix))]
[Shared]
internal class ImplementValueConverterFix : DocumentEditorCodeFixProvider
{
    private static readonly MethodDeclarationSyntax IMultiValueConverterConvert = ParseMethod(
        @"        public object Convert(object[] values, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }");

    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create("CS0535");

    protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
    {
        var document = context.Document;
        var syntaxRoot = await document.GetSyntaxRootAsync(context.CancellationToken)
                                       .ConfigureAwait(false);
        foreach (var diagnostic in context.Diagnostics)
        {
            if (syntaxRoot is { } &&
                syntaxRoot.TryFindNodeOrAncestor(diagnostic, out ClassDeclarationSyntax? classDeclaration))
            {
                if (HasInterface(classDeclaration, KnownSymbols.IValueConverter))
                {
                    if (diagnostic.GetMessage(CultureInfo.InvariantCulture)
                                  .Contains("does not implement interface member 'IValueConverter.Convert(object, Type, object, CultureInfo)'"))
                    {
                        context.RegisterCodeFix(
                            "Implement IValueConverter.Convert for one way bindings.",
                            (editor, _) => editor.AddMethod(classDeclaration, Convert(editor.Generator)),
                            "Implement IValueConverter",
                            diagnostic);
                    }

                    if (diagnostic.GetMessage(CultureInfo.InvariantCulture)
                                  .Contains("does not implement interface member 'IValueConverter.ConvertBack(object, Type, object, CultureInfo)'"))
                    {
                        context.RegisterCodeFix(
                            "Implement IValueConverter.ConvertBack for one way bindings.",
                            (editor, _) => editor.AddMethod(classDeclaration, ConvertBack(editor.Generator, classDeclaration.Identifier.ValueText)),
                            "Implement IValueConverter",
                            diagnostic);
                    }
                }

                if (HasInterface(classDeclaration, KnownSymbols.IMultiValueConverter))
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
    }

    private static bool HasInterface(ClassDeclarationSyntax classDeclaration, QualifiedType type)
    {
        if (classDeclaration.BaseList is null)
        {
            return false;
        }

        foreach (var typeSyntax in classDeclaration.BaseList.Types)
        {
            switch (typeSyntax.Type)
            {
                case SimpleNameSyntax name when name.Identifier.ValueText == type.Type:
                case QualifiedNameSyntax { Right: { Identifier: { } right } } when right.ValueText == type.Type:
                    return true;
            }
        }

        return false;
    }

    private static MethodDeclarationSyntax Convert(SyntaxGenerator generator)
    {
        return (MethodDeclarationSyntax)generator.MethodDeclaration(
            accessibility: Accessibility.Public,
            returnType: SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)),
            name: "Convert",
            parameters: new[]
            {
                generator.ParameterDeclaration("value",      SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword))),
                generator.ParameterDeclaration("targetType", SyntaxFactory.ParseTypeName("System.Type").WithSimplifiedNames()),
                generator.ParameterDeclaration("parameter",  SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword))),
                generator.ParameterDeclaration("culture",    SyntaxFactory.ParseTypeName("System.Globalization.CultureInfo").WithSimplifiedNames()),
            },
            statements: new[] { generator.ThrowStatement(generator.ObjectCreationExpression(SyntaxFactory.ParseTypeName("System.NotImplementedException").WithSimplifiedNames())) });
    }

    private static MethodDeclarationSyntax ConvertBack(SyntaxGenerator generator, string containingTypeName)
    {
        return ((MethodDeclarationSyntax)generator.MethodDeclaration(
            returnType: SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)),
            name: "ConvertBack",
            parameters: new[]
            {
                generator.ParameterDeclaration("value",      SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword))),
                generator.ParameterDeclaration("targetType", SyntaxFactory.ParseTypeName("System.Type").WithSimplifiedNames()),
                generator.ParameterDeclaration("parameter",  SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword))),
                generator.ParameterDeclaration("culture",    SyntaxFactory.ParseTypeName("System.Globalization.CultureInfo").WithSimplifiedNames()),
            },
            statements: new[] { Throw() }))
            .WithExplicitInterfaceSpecifier(SyntaxFactory.ExplicitInterfaceSpecifier(SyntaxFactory.ParseName("IValueConverter")));

        ThrowStatementSyntax Throw()
        {
            return (ThrowStatementSyntax)generator.ThrowStatement(
                generator.ObjectCreationExpression(
                    SyntaxFactory.ParseTypeName("System.NotSupportedException"),
                    SyntaxFactory.Argument(
                        expression: SyntaxFactory.InterpolatedStringExpression(
                            stringStartToken: SyntaxFactory.Token(SyntaxKind.InterpolatedStringStartToken),
                            contents: SyntaxFactory.List(
                                new InterpolatedStringContentSyntax[]
                                {
                                    SyntaxFactory.Interpolation(
                                        openBraceToken: SyntaxFactory.Token(SyntaxKind.OpenBraceToken),
                                        expression: (ExpressionSyntax)generator.NameOfExpression(SyntaxFactory.ParseTypeName(containingTypeName)),
                                        alignmentClause: default,
                                        formatClause: default,
                                        closeBraceToken: SyntaxFactory.Token(SyntaxKind.CloseBraceToken)),
                                    SyntaxFactory.InterpolatedStringText(
                                        textToken: SyntaxFactory.Token(
                                            leading: default,
                                            kind: SyntaxKind.InterpolatedStringTextToken,
                                            text: " can only be used in OneWay bindings",
                                            valueText: " can only be used in OneWay bindings",
                                            trailing: default)),
                                }),
                            stringEndToken: SyntaxFactory.Token(SyntaxKind.InterpolatedStringEndToken)))));
        }
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
