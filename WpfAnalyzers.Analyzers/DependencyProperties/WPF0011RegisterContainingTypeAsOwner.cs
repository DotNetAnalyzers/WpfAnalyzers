namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0011RegisterContainingTypeAsOwner : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0011";
        private const string Title = "DependencyProperty must be registered for containing type.";
        private const string MessageFormat = "DependencyProperty '{0}' must be registered for {1}";
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

            TypeSyntax registeredOwnerType;
            if (!declaration.TryGetDependencyPropertyRegisteredOwnerType(out registeredOwnerType))
            {
                return;
            }

            var semanticModel = context.SemanticModel;
            var containingType = fieldSymbol.ContainingSymbol as ITypeSymbol;
            if (!TypeHelper.IsSameType(semanticModel?.GetTypeInfo(registeredOwnerType).Type, containingType))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, registeredOwnerType.GetLocation(), fieldSymbol, containingType));
            }
        }
    }
}