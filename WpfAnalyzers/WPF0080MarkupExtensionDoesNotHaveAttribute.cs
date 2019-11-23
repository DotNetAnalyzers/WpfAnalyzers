namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0080MarkupExtensionDoesNotHaveAttribute : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Descriptors.WPF0080MarkupExtensionDoesNotHaveAttribute);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(x => Handle(x), SyntaxKind.ClassDeclaration);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.ContainingSymbol is INamedTypeSymbol { IsGenericType: false } type &&
                context.Node is ClassDeclarationSyntax classDeclaration &&
                type.IsAssignableTo(KnownSymbols.MarkupExtension, context.Compilation) &&
                type.TryFindFirstMethod("ProvideValue", x => x.Parameters.TrySingle(out var parameter) && parameter.Type == KnownSymbols.IServiceProvider, out _) &&
                !Attribute.TryFind(classDeclaration, KnownSymbols.MarkupExtensionReturnTypeAttribute, context.SemanticModel, context.CancellationToken, out _))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0080MarkupExtensionDoesNotHaveAttribute, classDeclaration.Identifier.GetLocation()));
            }
        }
    }
}
