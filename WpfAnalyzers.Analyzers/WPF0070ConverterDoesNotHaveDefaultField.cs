namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0070ConverterDoesNotHaveDefaultField : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0070";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Add default field to converter.",
            messageFormat: "Add default field to converter.",
            category: AnalyzerCategory.IValueConverter,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "Add default field to converter.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(HandleDeclaration, SyntaxKind.ClassDeclaration);
        }

        private static void HandleDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.ContainingSymbol is ITypeSymbol type &&
                context.Node is ClassDeclarationSyntax classDeclaration &&
                !type.IsAbstract &&
                type.IsEither(KnownSymbol.IValueConverter, KnownSymbol.IMultiValueConverter) &&
                !type.Is(KnownSymbol.MarkupExtension) &&
                !Mutable.HasMutableInstanceMembers(type) &&
                !Virtual.HasVirtualOrAbstractOrProtectedMembers(type) &&
                !ValueConverter.TryGetDefaultFieldsOrProperties(type, out _))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, classDeclaration.Identifier.GetLocation()));
            }
        }
    }
}