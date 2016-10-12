namespace WpfAnalyzers.DependencyProperties
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
        private const string Title = "DependencyPropertyKey field must come before DependencyProperty field.";
        private const string MessageFormat = "Field '{0}' must come before '{1}'";
        private const string Description = Title;
        private static readonly string HelpLink = WpfAnalyzers.HelpLink.ForId(DiagnosticId);

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
                                                                      DiagnosticId,
                                                                      Title,
                                                                      MessageFormat,
                                                                      AnalyzerCategory.DependencyProperties,
                                                                      DiagnosticSeverity.Error,
                                                                      AnalyzerConstants.EnabledByDefault,
                                                                      Description,
                                                                      HelpLink);

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

            IFieldSymbol keyField;
            if (!DependencyProperty.TryGetDependencyPropertyKeyField(
                  field,
                  context.SemanticModel,
                  context.CancellationToken,
                  out keyField))
            {
                return;
            }

            SyntaxReference reference;
            if (keyField.DeclaringSyntaxReferences.TryGetFirst(out reference))
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