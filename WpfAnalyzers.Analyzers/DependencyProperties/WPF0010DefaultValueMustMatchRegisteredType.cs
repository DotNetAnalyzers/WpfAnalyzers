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
            var typeSymbol = context.SemanticModel.GetTypeInfo(objectCreation).Type;
            var propertyMetadata = context.Compilation.GetTypeByMetadataName("System.Windows.PropertyMetadata");
            if (!typeSymbol.IsAssignableTo(propertyMetadata) ||
                objectCreation?.ArgumentList.Arguments.FirstOrDefault() == null)
            {
                return;
            }

            if (!propertyMetadata.ContainingNamespace.Equals(typeSymbol.ContainingNamespace))
            {
                // don't think there is a way to handle custom subclassed.
                // should not be common
                return;
            }

            InvocationExpressionSyntax registerCall;
            if (!TryGetRegisterCall(objectCreation, out registerCall))
            {
                return;
            }

            var registerSymbol = context.SemanticModel.GetSymbolInfo(registerCall, context.CancellationToken).Symbol;
            if (registerSymbol == null ||
                !registerSymbol.Name.StartsWith(Names.Register) ||
                registerSymbol.ContainingType.Name != Names.DependencyProperty)
            {
                return;
            }

            ITypeSymbol type;
            if (!registerCall.ArgumentList.Arguments[1].TryGetType(context.SemanticModel, context.CancellationToken, out type))
            {
                return;
            }

            var defaultValue = objectCreation.ArgumentList.Arguments[0].Expression;
            if (
                !type.IsRepresentationConservingConversion(
                    defaultValue,
                    context.SemanticModel,
                    context.CancellationToken))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, defaultValue.GetLocation(), registerCall.Ancestors().OfType<FieldDeclarationSyntax>().FirstOrDefault()?.Name(), type));
            }
        }

        private static bool TryGetRegisterCall(ObjectCreationExpressionSyntax objectCreation, out InvocationExpressionSyntax result)
        {
            result = null;
            var argumentSyntax = objectCreation?.Parent as ArgumentSyntax;
            var argumentList = argumentSyntax?.Parent as ArgumentListSyntax;
            if (argumentList == null)
            {
                return false;
            }

            result = argumentList.Parent as InvocationExpressionSyntax;
            return result != null;
        }
    }
}