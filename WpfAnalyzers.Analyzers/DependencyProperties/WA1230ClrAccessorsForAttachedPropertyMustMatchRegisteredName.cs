namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WA1230ClrAccessorsForAttachedPropertyMustMatchRegisteredName : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WA1230";
        private const string Title = "CLR accessor for attached property must match registered name.";
        private const string MessageFormat = "Method '{0}' must be named '{1}'";
        private const string Description = Title;
        private const string HelpLink = "http://stackoverflow.com/";

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
            if (method.TryGetDependencyPropertyRegisteredNameFromAttachedGet(out registeredName))
            {
                if (!IsMatchingGetName(methodName, registeredName))
                {
                    var identifier = method.Identifier;
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, identifier.GetLocation(), methodName, "Get" + registeredName));
                }
            }
            else if (method.TryGetDependencyPropertyRegisteredNameFromAttachedSet(out registeredName))
            {
                if (!IsMatchingSetName(methodName, registeredName))
                {
                    var identifier = method.Identifier;
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, identifier.GetLocation(), methodName, "Set" + registeredName));
                }
            }
        }

        private static bool IsMatchingGetName(string methodName, string registeredName)
        {
            if (methodName.Length != registeredName.Length + 3)
            {
                return false;
            }

            return methodName.StartsWith("Get") && methodName.EndsWith(registeredName);
        }

        private static bool IsMatchingSetName(string methodName, string registeredName)
        {
            if (methodName.Length != registeredName.Length + 3)
            {
                return false;
            }

            return methodName.StartsWith("Set") && methodName.EndsWith(registeredName);
        }
    }
}