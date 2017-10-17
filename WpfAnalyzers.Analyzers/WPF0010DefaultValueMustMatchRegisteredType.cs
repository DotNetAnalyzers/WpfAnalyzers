namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0010DefaultValueMustMatchRegisteredType : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0010";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Default value type must match registered type.",
            messageFormat: "Default value for '{0}' must be of type {1}",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "A DependencyProperty is registered with a type and a default value. The type of the default value must be the same as the registered type.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(HandleObjectCreation, SyntaxKind.ObjectCreationExpression);
        }

        private static void HandleObjectCreation(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            var objectCreation = context.Node as ObjectCreationExpressionSyntax;
            if (objectCreation == null ||
                objectCreation.IsMissing)
            {
                return;
            }

            if (!PropertyMetaData.TryGetDefaultValue(
        objectCreation,
        context.SemanticModel,
        context.CancellationToken,
        out ArgumentSyntax defaultValueArg))
            {
                return;
            }

            var defaultValue = defaultValueArg.Expression;
            if (defaultValue.IsSameType(KnownSymbol.Object, context))
            {
                return;
            }

            if (!PropertyMetaData.TryGetDependencyProperty(objectCreation, context.SemanticModel, context.CancellationToken, out IFieldSymbol dp))
            {
                return;
            }

            if (!DependencyProperty.TryGetRegisteredType(dp, context.SemanticModel, context.CancellationToken, out ITypeSymbol registeredType))
            {
                return;
            }

            if (!registeredType.IsRepresentationPreservingConversion(defaultValue, context.SemanticModel, context.CancellationToken))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, defaultValueArg.GetLocation(), dp, registeredType));
            }
        }
    }
}