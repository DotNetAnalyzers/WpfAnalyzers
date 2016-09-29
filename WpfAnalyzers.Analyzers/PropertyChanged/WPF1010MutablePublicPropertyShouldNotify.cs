namespace WpfAnalyzers.PropertyChanged
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using WpfAnalyzers.PropertyChanged.Helpers;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF1010MutablePublicPropertyShouldNotify : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF1010";
        private const string Title = "Mutable public property should notify.";
        private const string MessageFormat = "Property '{0}' must notify when value changes.";
        private const string Description = "All mutable public properties should notify when their value changes.";
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
            context.RegisterSyntaxNodeAction(HandleDeclaration, SyntaxKind.PropertyDeclaration);
        }

        private static void HandleDeclaration(SyntaxNodeAnalysisContext context)
        {
            var propertySymbol = (IPropertySymbol)context.ContainingSymbol;
            if (propertySymbol.IsIndexer ||
                propertySymbol.DeclaredAccessibility != Accessibility.Public ||
                propertySymbol.IsStatic ||
                propertySymbol.IsReadOnly ||
                propertySymbol.IsAbstract ||
                propertySymbol.ContainingType.IsValueType ||
                propertySymbol.ContainingType.DeclaredAccessibility != Accessibility.Public)
            {
                return;
            }

            var declaration = (PropertyDeclarationSyntax)context.Node;
            if (IsAutoProperty(declaration))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, declaration.GetLocation(), context.ContainingSymbol.Name));
                return;
            }

            AccessorDeclarationSyntax setter;
            if (declaration.TryGetSetAccessorDeclaration(out setter))
            {
                if (!AssignsValueToBackingField(setter))
                {
                    return;
                }

                if (setter.InvokesPropertyChangedFor(propertySymbol, context.SemanticModel, context.CancellationToken) != PropertyChanged.InvokesPropertyChanged.No)
                {
                    return;
                }

                context.ReportDiagnostic(Diagnostic.Create(Descriptor, declaration.GetLocation(), context.ContainingSymbol.Name));
            }
        }

        private static bool IsAutoProperty(PropertyDeclarationSyntax property)
        {
            var accessors = property?.AccessorList?.Accessors;
            if (accessors?.Count != 2)
            {
                return false;
            }

            foreach (var accessor in accessors.Value)
            {
                if (accessor.Body != null)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool AssignsValueToBackingField(AccessorDeclarationSyntax setter)
        {
            using (var pooled = AssignmentWalker.Create(setter))
            {
                foreach (var assignment in pooled.Item.Assignments)
                {
                    if ((assignment.Right as IdentifierNameSyntax)?.Identifier.ValueText != "value")
                    {
                        continue;
                    }

                    if (assignment.Left is IdentifierNameSyntax)
                    {
                        return true;
                    }

                    var memberAccess = assignment.Left as MemberAccessExpressionSyntax;
                    if (memberAccess?.Expression is ThisExpressionSyntax &&
                        memberAccess.Name is IdentifierNameSyntax)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}