namespace WpfAnalyzers;

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;

using Gu.Roslyn.CodeFixExtensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ValueConversionAttributeFix))]
[Shared]
internal class ValueConversionAttributeFix : DocumentEditorCodeFixProvider
{
    private static readonly AttributeSyntax Attribute = SyntaxFactory
                                                        .Attribute(SyntaxFactory.ParseName("System.Windows.Data.ValueConversionAttribute")).WithSimplifiedNames();

    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
        Descriptors.WPF0071ConverterDoesNotHaveAttribute.Id,
        Descriptors.WPF0073ConverterDoesNotHaveAttributeUnknownTypes.Id);

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
                syntaxRoot.TryFindNodeOrAncestor<ClassDeclarationSyntax>(diagnostic, out var classDeclaration))
            {
                if (ValueConverter.TryGetConversionTypes(classDeclaration, semanticModel, context.CancellationToken, out var sourceType, out var targetType))
                {
                    context.RegisterCodeFix(
                        $"Add [ValueConversion(typeof({sourceType}), typeof({targetType}))].",
                        (e, _) => AddAttribute(e, classDeclaration, sourceType, targetType),
                        $"Add [ValueConversion(typeof({sourceType}), typeof({targetType}))].",
                        diagnostic);
                }
                else
                {
                    context.RegisterCodeFix(
                        $"Add [ValueConversion(typeof({sourceType?.ToString() ?? "TYPE"}), typeof({targetType?.ToString() ?? "TYPE"}))].",
                        (e, _) => AddAttribute(e, classDeclaration, sourceType, targetType),
                        $"Add [ValueConversion(typeof({sourceType?.ToString() ?? "TYPE"}), typeof({targetType?.ToString() ?? "TYPE"}))].",
                        diagnostic);
                }
            }
        }
    }

    private static void AddAttribute(DocumentEditor editor, ClassDeclarationSyntax classDeclaration, ITypeSymbol? sourceType, ITypeSymbol? targetType)
    {
        var attributeArguments = new[]
        {
            editor.Generator.AttributeArgument(TypeOf(sourceType)),
            editor.Generator.AttributeArgument(TypeOf(targetType)),
        };
        editor.AddAttribute(
            classDeclaration,
            editor.Generator.AddAttributeArguments(
                Attribute,
                attributeArguments));

        TypeOfExpressionSyntax TypeOf(ITypeSymbol? t)
        {
            if (t is { })
            {
                return (TypeOfExpressionSyntax)editor.Generator.TypeOfExpression(editor.Generator.TypeExpression(t));
            }

            return (TypeOfExpressionSyntax)editor.Generator.TypeOfExpression(SyntaxFactory.ParseTypeName("TYPE"));
        }
    }
}
