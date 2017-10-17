namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0031FieldOrder : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0031";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "DependencyPropertyKey field must come before DependencyProperty field.",
            messageFormat: "Field '{0}' must come before '{1}'",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "DependencyPropertyKey field must come before DependencyProperty field.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(HandleDeclaration, SyntaxKind.FieldDeclaration);
        }

        private static void HandleDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            var fieldDeclaration = context.Node as FieldDeclarationSyntax;
            if (fieldDeclaration == null ||
                fieldDeclaration.IsMissing)
            {
                return;
            }

            var field = context.ContainingSymbol as IFieldSymbol;
            if (field == null ||
                !DependencyProperty.IsPotentialDependencyPropertyBackingField(field))
            {
                return;
            }

            if (!DependencyProperty.TryGetDependencyPropertyKeyField(
      field,
      context.SemanticModel,
      context.CancellationToken,
      out IFieldSymbol keyField))
            {
                return;
            }

            if (field.ContainingType != keyField.ContainingType)
            {
                return;
            }

            if (keyField.DeclaringSyntaxReferences.TryGetFirst(out SyntaxReference reference))
            {
                var keyNode = reference.GetSyntax(context.CancellationToken);
                if (!ReferenceEquals(fieldDeclaration.SyntaxTree, keyNode.SyntaxTree) ||
                    fieldDeclaration.SpanStart < keyNode.SpanStart)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, fieldDeclaration.GetLocation(), keyField.Name, field.Name));
                }
            }
        }
    }
}