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
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create("CS0535");

    protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
    {
        var document = context.Document;
        var syntaxRoot = await document.GetSyntaxRootAsync(context.CancellationToken)
                                       .ConfigureAwait(false);

        var semanticModel = await document.GetSemanticModelAsync(context.CancellationToken)
                                       .ConfigureAwait(false);
        foreach (var diagnostic in context.Diagnostics)
        {
            if (syntaxRoot is { } &&
                semanticModel is { } &&
                syntaxRoot.TryFindNodeOrAncestor(diagnostic, out ClassDeclarationSyntax? classDeclaration))
            {
                var nullableContext = semanticModel.GetNullableContext(diagnostic.Location.SourceSpan.Start);
                if (HasInterface(classDeclaration, KnownSymbols.IValueConverter))
                {
                    if (diagnostic.GetMessage(CultureInfo.InvariantCulture)
                                  .Contains("does not implement interface member 'IValueConverter.Convert(object, Type, object, CultureInfo)'"))
                    {
                        context.RegisterCodeFix(
                            "Implement IValueConverter.Convert for one way bindings.",
                            (editor, _) =>
                            {
                                editor.AddMethod(classDeclaration, IValueConverter.Convert(editor.Generator, nullableContext));
                            },
                            "Implement IValueConverter",
                            diagnostic);
                    }

                    if (diagnostic.GetMessage(CultureInfo.InvariantCulture)
                                  .Contains("does not implement interface member 'IValueConverter.ConvertBack(object, Type, object, CultureInfo)'"))
                    {
                        context.RegisterCodeFix(
                            "Implement IValueConverter.ConvertBack for one way bindings.",
                            (editor, _) => editor.AddMethod(classDeclaration, IValueConverter.ConvertBack(editor.Generator, classDeclaration.Identifier.ValueText, nullableContext)),
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
                            (editor, _) => editor.AddMethod(classDeclaration, IMultiValueConverter.Convert(editor.Generator, nullableContext)),
                            "Implement IMultiValueConverter",
                            diagnostic);
                    }

                    if (diagnostic.GetMessage(CultureInfo.InvariantCulture)
                                  .Contains("does not implement interface member 'IMultiValueConverter.ConvertBack(object, Type[], object, CultureInfo)'"))
                    {
                        context.RegisterCodeFix(
                            "Implement IMultiValueConverter.ConvertBack for one way bindings.",
                            (editor, _) => editor.AddMethod(classDeclaration, IMultiValueConverter.ConvertBack(classDeclaration.Identifier.ValueText)),
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
                case QualifiedNameSyntax { Right.Identifier: { } right } when right.ValueText == type.Type:
                    return true;
            }
        }

        return false;
    }

    private static TypeSyntax ParseTypeName(string text) => SyntaxFactory.ParseTypeName(text).WithSimplifiedNames();

    private static TypeSyntax Object(NullableContext nullableContext) => nullableContext switch
    {
        NullableContext.WarningsEnabled => SyntaxFactory.NullableType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword))),
        _ => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)),
    };

    private static class IValueConverter
    {
        internal static MethodDeclarationSyntax Convert(SyntaxGenerator generator, NullableContext nullableContext)
        {
            return (MethodDeclarationSyntax)generator.MethodDeclaration(
                accessibility: Accessibility.Public,
                returnType: Object(nullableContext),
                name: "Convert",
                parameters: new[]
                {
                generator.ParameterDeclaration("value",      Object(nullableContext)),
                generator.ParameterDeclaration("targetType", ParseTypeName("System.Type")),
                generator.ParameterDeclaration("parameter",  Object(nullableContext)),
                generator.ParameterDeclaration("culture",    ParseTypeName("System.Globalization.CultureInfo")),
                },
                statements: new[] { generator.ThrowStatement(generator.ObjectCreationExpression(ParseTypeName("System.NotImplementedException"))) });
        }

        internal static MethodDeclarationSyntax ConvertBack(SyntaxGenerator generator, string containingTypeName, NullableContext nullableContext)
        {
            return ((MethodDeclarationSyntax)generator.MethodDeclaration(
                returnType: Object(nullableContext),
                name: "ConvertBack",
                parameters: new[]
                {
                generator.ParameterDeclaration("value",      Object(nullableContext)),
                generator.ParameterDeclaration("targetType", ParseTypeName("System.Type")),
                generator.ParameterDeclaration("parameter",  Object(nullableContext)),
                generator.ParameterDeclaration("culture",    ParseTypeName("System.Globalization.CultureInfo")),
                },
                statements: new[] { Throw() }))
                .WithExplicitInterfaceSpecifier(SyntaxFactory.ExplicitInterfaceSpecifier(SyntaxFactory.ParseName("IValueConverter")));

            ThrowStatementSyntax Throw()
            {
                return (ThrowStatementSyntax)generator.ThrowStatement(
                    generator.ObjectCreationExpression(
                        ParseTypeName("System.NotSupportedException"),
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
    }

    private static class IMultiValueConverter
    {
        internal static MethodDeclarationSyntax Convert(SyntaxGenerator generator, NullableContext nullableContext)
        {
            return (MethodDeclarationSyntax)generator.MethodDeclaration(
                accessibility: Accessibility.Public,
                returnType: Object(nullableContext),
                name: "Convert",
                parameters: new[]
                {
                    generator.ParameterDeclaration("value",      SyntaxFactory.ArrayType(Object(nullableContext))),
                    generator.ParameterDeclaration("targetType", ParseTypeName("System.Type")),
                    generator.ParameterDeclaration("parameter",  Object(nullableContext)),
                    generator.ParameterDeclaration("culture",    ParseTypeName("System.Globalization.CultureInfo")),
                },
                statements: new[] { generator.ThrowStatement(generator.ObjectCreationExpression(ParseTypeName("System.NotImplementedException"))) });
        }

        internal static MethodDeclarationSyntax ConvertBack(string containingTypeName)
        {
            var code = StringBuilderPool.Borrow()
                                        .AppendLine("        object[] System.Windows.Data.IMultiValueConverter.ConvertBack(object value, System.Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)")
                                        .AppendLine("        {")
                                        .AppendLine($"            throw new System.NotSupportedException($\"{{nameof({containingTypeName})}} can only be used in OneWay bindings\");")
                                        .AppendLine("        }")
                                        .Return();
            return ParseMethod(code);

            static MethodDeclarationSyntax ParseMethod(string code)
            {
                return Parse.MethodDeclaration(code)
                            .WithSimplifiedNames()
                            .WithLeadingTrivia(SyntaxFactory.ElasticMarker)
                            .WithTrailingTrivia(SyntaxFactory.ElasticMarker);
            }
        }
    }
}
