namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0011ContainingTypeShouldBeRegisteredOwner : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0011";
        private const string Title = "Containing type should be used as registered owner.";
        private const string MessageFormat = "DependencyProperty '{0}' must be registered for {1}";
        private const string Description = "When registering a DependencyProperty register containing type as owner type.";
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
            context.RegisterSyntaxNodeAction(HandleDeclaration, SyntaxKind.FieldDeclaration);
        }

        private static void HandleDeclaration(SyntaxNodeAnalysisContext context)
        {
            var declaration = context.Node as FieldDeclarationSyntax;
            if (declaration == null ||
                declaration.IsMissing ||
                !(declaration.IsDependencyPropertyField() ||
                declaration.IsDependencyPropertyKeyField()))
            {
                return;
            }

            var fieldSymbol = context.ContainingSymbol as IFieldSymbol;
            if (fieldSymbol == null)
            {
                return;
            }

            FieldDeclarationSyntax dependencyPropertyKey;
            if (declaration.IsDependencyPropertyField() && declaration.TryGetDependencyPropertyKey(out dependencyPropertyKey))
            {
                return;
            }

            ITypeSymbol registeredOwnerType;
            ArgumentSyntax arg;
            if (!declaration.TryGetDependencyPropertyRegisteredOwnerType(context.SemanticModel, out arg, out registeredOwnerType))
            {
                return;
            }

            var containingType = fieldSymbol.ContainingSymbol as ITypeSymbol;
            if (!TypeHelper.IsSameType(registeredOwnerType, containingType))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, arg.GetLocation(), fieldSymbol, containingType));
            }
        }
    }
}