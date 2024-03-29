﻿namespace WpfAnalyzers;

using System.Collections.Immutable;

using Gu.Roslyn.AnalyzerExtensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class WPF0083UseConstructorArgumentAttribute : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Descriptors.WPF0083UseConstructorArgumentAttribute);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(x => Handle(x), SyntaxKind.PropertyDeclaration);
    }

    private static void Handle(SyntaxNodeAnalysisContext context)
    {
        if (!context.IsExcludedFromAnalysis() &&
            context is { Node: PropertyDeclarationSyntax propertyDeclaration, ContainingSymbol: IPropertySymbol property } &&
            property.ContainingType.IsAssignableTo(KnownSymbols.MarkupExtension, context.SemanticModel.Compilation) &&
            !Attribute.TryFind(propertyDeclaration, KnownSymbols.ConstructorArgumentAttribute, context.SemanticModel, context.CancellationToken, out _) &&
            ConstructorArgument.TryGetParameterName(property, context.SemanticModel, context.CancellationToken, out var parameterName))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    Descriptors.WPF0083UseConstructorArgumentAttribute,
                    propertyDeclaration.Identifier.GetLocation(),
                    ImmutableDictionary<string, string?>.Empty.Add(nameof(ConstructorArgument), parameterName),
                    parameterName));
        }
    }
}
