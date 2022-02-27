namespace WpfAnalyzers;

using System.Collections.Immutable;

using Gu.Roslyn.AnalyzerExtensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class ComponentResourceKeyAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Descriptors.WPF0140UseContainingTypeComponentResourceKey,
        Descriptors.WPF0141UseContainingMemberComponentResourceKey);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(c => Handle(c), SyntaxKind.ObjectCreationExpression);
    }

    private static void Handle(SyntaxNodeAnalysisContext context)
    {
        if (!context.IsExcludedFromAnalysis() &&
            context.Node is ObjectCreationExpressionSyntax { ArgumentList: { } argumentList } objectCreation &&
            context.ContainingSymbol is { ContainingType: { } containingType } &&
            objectCreation.Type == KnownSymbols.ComponentResourceKey &&
            context.SemanticModel.TryGetSymbol(objectCreation, KnownSymbols.ComponentResourceKey, context.CancellationToken, out var constructor) &&
            FieldOrProperty.TryCreate(context.ContainingSymbol, out var fieldOrProperty))
        {
            if (constructor.Parameters.Length == 0)
            {
                var containingTypeString = containingType.ToMinimalDisplayString(context.SemanticModel, objectCreation.SpanStart);
                var argumentListText = $"typeof({containingTypeString}), nameof({fieldOrProperty.Name})";
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptors.WPF0140UseContainingTypeComponentResourceKey,
                        argumentList.GetLocation(),
                        ImmutableDictionary<string, string?>.Empty.Add(nameof(ArgumentListSyntax), argumentListText),
                        argumentListText));
            }
            else
            {
                if (constructor.TryFindParameter("typeInTargetAssembly", out var parameter) &&
                    objectCreation.TryFindArgument(parameter, out var arg) &&
                    arg.TryGetTypeofValue(context.SemanticModel, context.CancellationToken, out var type) &&
                    !TypeSymbolComparer.Equal(type, containingType))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptors.WPF0140UseContainingTypeComponentResourceKey,
                            arg.GetLocation(),
                            containingType.ToMinimalDisplayString(context.SemanticModel, objectCreation.SpanStart)));
                }

                if (constructor.TryFindParameter("resourceId", out parameter) &&
                    objectCreation.TryFindArgument(parameter, out arg) &&
                    context.SemanticModel.TryGetConstantValue(arg.Expression, context.CancellationToken, out string? name) &&
                    name != fieldOrProperty.Name)
                {
                    var keyText = $"nameof({fieldOrProperty.Name})";
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptors.WPF0141UseContainingMemberComponentResourceKey,
                            arg.GetLocation(),
                            ImmutableDictionary<string, string?>.Empty.Add(nameof(ArgumentSyntax), keyText),
                            keyText));
                }
            }
        }
    }
}