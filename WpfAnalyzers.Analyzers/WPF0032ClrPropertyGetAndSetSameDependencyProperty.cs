namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0032ClrPropertyGetAndSetSameDependencyProperty : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0032";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            DiagnosticId,
            "Use same dependency property in get and set.",
            "Property '{0}' must access same dependency property in getter and setter",
            AnalyzerCategory.DependencyProperties,
            DiagnosticSeverity.Error,
            AnalyzerConstants.EnabledByDefault,
            "Use same dependency property in get and set.",
            HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(HandleDeclaration, SyntaxKind.PropertyDeclaration);
        }

        private static void HandleDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.Node is PropertyDeclarationSyntax propertyDeclaration &&
                ClrProperty.TryGetBackingFields(propertyDeclaration, context.SemanticModel, context.CancellationToken, out var getter, out var setter))
            {
                if (DependencyProperty.TryGetDependencyPropertyKeyField(
                    getter,
                    context.SemanticModel,
                    context.CancellationToken,
                    out var keyField))
                {
                    getter = keyField;
                }

                if (ReferenceEquals(getter.Symbol, setter.Symbol))
                {
                    return;
                }

                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptor,
                        propertyDeclaration.GetLocation(),
                        context.ContainingSymbol.Name));
            }
        }
    }
}