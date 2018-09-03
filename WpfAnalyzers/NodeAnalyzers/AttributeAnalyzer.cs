namespace WpfAnalyzers
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class AttributeAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(WPF0082ConstructorArgument.Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(x => Handle(x), SyntaxKind.Attribute);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is AttributeSyntax attribute &&
                Attribute.IsType(attribute, KnownSymbol.ConstructorArgumentAttribute, context.SemanticModel, context.CancellationToken) &&
                ConstructorArgument.TryGetArgumentName(attribute, out var argument, out var argumentName) &&
                attribute.TryFirstAncestor<PropertyDeclarationSyntax>(out var propertyDeclaration) &&
                context.SemanticModel.TryGetSymbol(propertyDeclaration, context.CancellationToken, out var property) &&
                ConstructorArgument.TryGetParameterName(property, context.SemanticModel, context.CancellationToken, out var parameterName) &&
                argumentName != parameterName)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        WPF0082ConstructorArgument.Descriptor,
                        argument.GetLocation(),
                        ImmutableDictionary.CreateRange(new[] { new KeyValuePair<string, string>(nameof(ConstructorArgument), parameterName) }),
                        parameterName));
            }
        }
    }
}