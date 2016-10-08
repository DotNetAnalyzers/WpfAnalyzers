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
        private const string Description = "CLR methods for accessing a DependencyProperty must have names matching the name the DependencyProperty is registered with.";
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
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(HandleDeclaration, SyntaxKind.MethodDeclaration);
        }

        private static void HandleDeclaration(SyntaxNodeAnalysisContext context)
        {
            var method = context.Node as MethodDeclarationSyntax;
            if (method == null || method.IsMissing)
            {
                return;
            }

            var methodName = method.Name();
            if (methodName == null)
            {
                return;
            }

            string registeredName;
            if (method.TryGetDependencyPropertyRegisteredNameFromAttachedGet(context.SemanticModel, out registeredName))
            {
                if (!methodName.IsParts("Get", registeredName))
                {
                    var identifier = method.Identifier;
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, identifier.GetLocation(), methodName, "Get" + registeredName));
                }
            }
            else if (method.TryGetDependencyPropertyRegisteredNameFromAttachedSet(context.SemanticModel, out registeredName))
            {
                if (!methodName.IsParts("Set", registeredName))
                {
                    var identifier = method.Identifier;
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, identifier.GetLocation(), methodName, "Set" + registeredName));
                }
            }
        }
    }
}