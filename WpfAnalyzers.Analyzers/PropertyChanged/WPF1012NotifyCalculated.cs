namespace WpfAnalyzers.PropertyChanged
{
    using System.Collections.Immutable;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    using WpfAnalyzers.PropertyChanged.Helpers;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF1012NotifyCalculated : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF1012";
        private const string Title = "Notify when calculated property changes.";
        private const string MessageFormat = "Notify when calculated property changes.";
        private const string Description = "Notify when calculated property changes.";
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
            var declaration = (PropertyDeclarationSyntax)context.Node;
            AccessorDeclarationSyntax _;
            if (declaration.TryGetSetAccessorDeclaration(out _) ||
                !propertySymbol.ContainingType.Is(KnownSymbol.INotifyPropertyChanged))
            {
                return;
            }

            SyntaxNode body;
            AccessorDeclarationSyntax getter;
            if (declaration.TryGetGetAccessorDeclaration(out getter))
            {
                body = getter.Body;
            }
            else
            {
                body = declaration.ExpressionBody;
            }

            if (body == null)
            {
                return;
            }

            using (var pooled = IdentifierNameWalker.Create(body))
            {
                foreach (var name in pooled.Item.IdentifierNames)
                {
                    var symbol = context.SemanticModel.GetSymbolSafe(name, context.CancellationToken);
                    var field = symbol as IFieldSymbol;
                    if (field?.IsConst == true ||
                        field?.IsReadOnly == true ||
                        field?.IsStatic == true)
                    {
                        continue;
                    }

                    if (field == null)
                    {
                        var property = symbol as IPropertySymbol;
                        if (property?.IsReadOnly == true ||
                            property?.IsStatic == true)
                        {
                            continue;
                        }

                        if (!Property.TryGetBackingField(property, context.SemanticModel, context.CancellationToken, out field))
                        {
                            continue;
                        }
                    }

                    if (field == null)
                    {
                        continue;
                    }

                    if (symbol?.ContainingType != context.ContainingSymbol.ContainingType)
                    {
                        continue;
                    }

                    using (var pooledAssignments = AssignmentWalker.Create(declaration.FirstAncestorOrSelf<TypeDeclarationSyntax>()))
                    {
                        foreach (var assignment in pooledAssignments.Item.Assignments)
                        {
                            if (IsAssigning(assignment, name))
                            {

                            }
                        }
                    }
                }
            }

            if (Property.ShouldNotify(declaration, propertySymbol, context.SemanticModel, context.CancellationToken))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, declaration.GetLocation(), context.ContainingSymbol.Name));
            }
        }

        private static bool IsAssigning(AssignmentExpressionSyntax assignment, IdentifierNameSyntax identifier)
        {
            var left = assignment.Left;
            var leftIdentifier = left as IdentifierNameSyntax;
            if (leftIdentifier != null)
            {
                return leftIdentifier.IsEquivalentTo(identifier) == true;
            }

            var memberAccess = left as MemberAccessExpressionSyntax;
            if (memberAccess?.Expression is ThisExpressionSyntax &&
                memberAccess.Name.IsEquivalentTo(identifier))
            {
                return true;
            }

            return false;
        }
    }
}