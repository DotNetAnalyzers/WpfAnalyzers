namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0016DefaultValueIsSharedReferenceType : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0016";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Default value is shared reference type.",
            messageFormat: "Default value for '{0}' is a reference type that will be shared among all instances.",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "When registering a new instance of a reference type as default value the value is shared for all instances of the control.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Handle, SyntaxKind.ObjectCreationExpression);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.Node is ObjectCreationExpressionSyntax objectCreation &&
                context.ContainingSymbol.IsStatic &&
                PropertyMetadata.TryGetDependencyProperty(objectCreation, context.SemanticModel, context.CancellationToken, out var fieldOrProperty) &&
                DependencyProperty.TryGetRegisteredType(fieldOrProperty, context.SemanticModel, context.CancellationToken, out var registeredType) &&
                registeredType.IsReferenceType &&
                PropertyMetadata.TryGetDefaultValue(objectCreation, context.SemanticModel, context.CancellationToken, out var defaultValueArg))
            {
                var defaultValue = defaultValueArg.Expression;
                if (defaultValue == null ||
                    defaultValue.IsKind(SyntaxKind.NullLiteralExpression))
                {
                    return;
                }

                if (IsNonEmptyArrayCreation(defaultValue as ArrayCreationExpressionSyntax, context) ||
                    IsReferenceTypeCreation(defaultValue as ObjectCreationExpressionSyntax, context))
                {
                    if (registeredType == KnownSymbol.FontFamily)
                    {
                        return;
                    }

                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, defaultValueArg.GetLocation(), fieldOrProperty.Symbol));
                }
            }
        }

        private static bool IsNonEmptyArrayCreation(ArrayCreationExpressionSyntax arrayCreation, SyntaxNodeAnalysisContext context)
        {
            if (arrayCreation == null)
            {
                return false;
            }

            foreach (var rank in arrayCreation.Type.RankSpecifiers)
            {
                foreach (var size in rank.Sizes)
                {
                    var constantValue = context.SemanticModel.GetConstantValueSafe(size, context.CancellationToken);
                    if (!constantValue.HasValue)
                    {
                        return true;
                    }

                    return !Equals(constantValue.Value, 0);
                }
            }

            return false;
        }

        private static bool IsReferenceTypeCreation(ObjectCreationExpressionSyntax objectCreation, SyntaxNodeAnalysisContext context)
        {
            if (objectCreation == null)
            {
                return false;
            }

            var type = context.SemanticModel.GetTypeInfoSafe(objectCreation, context.CancellationToken)
                              .Type;
            if (type.IsValueType)
            {
                return false;
            }

            return true;
        }
    }
}