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
    internal class WPF0083UseConstructorArgumentAttribute : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0083";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Add [ConstructorArgument].",
            messageFormat: "Add [ConstructorArgument(\"{0}\"]",
            category: AnalyzerCategory.MarkupExtension,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Add [ConstructorArgument] for the property.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(x => Handle(x), SyntaxKind.PropertyDeclaration);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is PropertyDeclarationSyntax propertyDeclaration &&
                context.ContainingSymbol is IPropertySymbol property &&
                property.ContainingType.IsAssignableTo(KnownSymbol.MarkupExtension, context.Compilation) &&
                !Attribute.TryFind(propertyDeclaration, KnownSymbol.ConstructorArgumentAttribute, context.SemanticModel, context.CancellationToken, out _) &&
                ConstructorArgument.TryGetParameterName(property, context.SemanticModel, context.CancellationToken, out var parameterName))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptor,
                        propertyDeclaration.Identifier.GetLocation(),
                        ImmutableDictionary.CreateRange(new[] { new KeyValuePair<string, string>(nameof(ConstructorArgument), parameterName) }),
                        parameterName));
            }
        }
    }
}
