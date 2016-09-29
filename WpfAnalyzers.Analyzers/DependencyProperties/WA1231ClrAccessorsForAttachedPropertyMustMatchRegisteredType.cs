namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WA1231ClrAccessorsForAttachedPropertyMustMatchRegisteredType : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WA1231";
        private const string Title = "CLR accessor for attached property must match registered type.";
        private const string MessageFormat = "Method '{0}' must have signature '{1}'";
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

            TypeSyntax registeredType;

            var methodName = method.Name();
            if (method.TryGetDependencyPropertyRegisteredTypeFromAttachedGet(out registeredType))
            {
                if (!IsMatchingReturnType(method, registeredType))
                {
                    var signature = $"public static {registeredType} {methodName}({FormatFirstParameter((IMethodSymbol)context.ContainingSymbol)})";
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, method.ReturnType.GetLocation(), methodName, signature));
                }
            }
            else if (method.TryGetDependencyPropertyRegisteredTypeFromAttachedSet(out registeredType))
            {
                if (!IsMatchingSetValueType(method, registeredType))
                {
                    var signature = $"public static void {methodName}({FormatFirstParameter(context.ContainingSymbol as IMethodSymbol)}, {registeredType} value)";
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, method.ParameterList.Parameters[1].GetLocation(), methodName, signature));
                }
            }
        }

        private static bool IsMatchingReturnType(MethodDeclarationSyntax clrMethod, TypeSyntax registeredType)
        {
            return clrMethod.ReturnType.Name() == registeredType.Name();
        }

        private static bool IsMatchingSetValueType(MethodDeclarationSyntax clrMethod, TypeSyntax registeredType)
        {
            return clrMethod.ParameterList.Parameters[1].Type.Name() == registeredType.Name();
        }

        private static string FormatFirstParameter(IMethodSymbol method)
        {
            var parameter = method.Parameters[0];
            return method.IsExtensionMethod
                       ? $"this {parameter.Type} {parameter.Name}"
                       : $"{parameter.Type} {parameter.Name}";
        }
    }
}