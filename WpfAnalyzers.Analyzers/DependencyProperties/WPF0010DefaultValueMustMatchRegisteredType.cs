namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;
    using System.Linq;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0010DefaultValueMustMatchRegisteredType : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0010";
        private const string Title = "Default value type must match registered type.";
        private const string MessageFormat = "Default value for '{0}' must be of type {1}";
        private const string Description = "A DependencyProperty is registered with a type and a default value. The type of the default value must be the same as the registered type.";
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
            context.RegisterSyntaxNodeAction(HandleObjectCreation, SyntaxKind.ObjectCreationExpression);
        }

        private static void HandleObjectCreation(SyntaxNodeAnalysisContext context)
        {
            var objectCreation = context.Node as ObjectCreationExpressionSyntax;
            if (objectCreation == null ||
                objectCreation.IsMissing)
            {
                return;
            }

            ArgumentSyntax defaultValueArg;
            if (!PropertyMetaData.TryGetDefaultValue(
                    objectCreation,
                    context.SemanticModel,
                    context.CancellationToken,
                    out defaultValueArg))
            {
                return;
            }

            var defaultValue = defaultValueArg.Expression;
            if (context.SemanticModel.SemanticModelFor(defaultValue).GetTypeInfo(defaultValue, context.CancellationToken).Type.IsObject())
            {
                return;
            }

            IFieldSymbol dp;
            if (!PropertyMetaData.TryGetDependencyProperty(objectCreation, context.SemanticModel, context.CancellationToken, out dp))
            {
                return;
            }

            ITypeSymbol registeredType;
            if (!DependencyProperty.TryGetRegisteredType(dp, context.SemanticModel, context.CancellationToken, out registeredType))
            {
                return;
            }

            if (!registeredType.IsRepresentationConservingConversion(defaultValue, context.SemanticModel, context.CancellationToken))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, defaultValue.GetLocation(), dp, registeredType));
            }
        }
    }
}