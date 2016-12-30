namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0004ClrMethodShouldMatchRegisteredName : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0004";
        private const string Title = "CLR method for a DependencyProperty should match registered name.";
        private const string MessageFormat = "Method '{0}' must be named '{1}'";

        private const string Description =
            "CLR methods for accessing a DependencyProperty must have names matching the name the DependencyProperty is registered with.";

        private static readonly string HelpLink = WpfAnalyzers.HelpLink.ForId(DiagnosticId);

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            AnalyzerCategory.DependencyProperties,
            DiagnosticSeverity.Warning,
            AnalyzerConstants.EnabledByDefault,
            Description,
            HelpLink);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(HandleDeclaration, SyntaxKind.MethodDeclaration);
        }

        private static void HandleDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            var methodDeclaration = context.Node as MethodDeclarationSyntax;
            if (methodDeclaration == null ||
                methodDeclaration.IsMissing)
            {
                return;
            }

            var method = context.ContainingSymbol as IMethodSymbol;
            if (method == null)
            {
                return;
            }

            IFieldSymbol setField;
            if (ClrMethod.IsAttachedSetMethod(method, context.SemanticModel, context.CancellationToken, out setField))
            {
                CheckName(context, setField, method, methodDeclaration, "Set");
                return;
            }

            IFieldSymbol getField;
            if (ClrMethod.IsAttachedGetMethod(method, context.SemanticModel, context.CancellationToken, out getField))
            {
                CheckName(context, getField, method, methodDeclaration, "Get");
            }
        }

        private static void CheckName(
            SyntaxNodeAnalysisContext context,
            IFieldSymbol dependencyProperty,
            IMethodSymbol method,
            MethodDeclarationSyntax methodDeclaration,
            string prefix)
        {
            string registeredName;
            if (DependencyProperty.TryGetRegisteredName(
                dependencyProperty,
                context.SemanticModel,
                context.CancellationToken,
                out registeredName))
            {
                if (!method.Name.IsParts(prefix, registeredName))
                {
                    var identifier = methodDeclaration.Identifier;
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptor,
                            identifier.GetLocation(),
                            method.Name,
                            prefix + registeredName));
                }
            }
        }
    }
}