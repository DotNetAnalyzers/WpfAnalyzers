namespace WpfAnalyzers.PropertyChanged
{
    using System.Collections.Generic;
    using System.Collections.Immutable;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    using WpfAnalyzers.PropertyChanged.Helpers;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF1012NotifyWhenPropertyChanges : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF1012";
        public static readonly string PropertyNameKey = "PropertyName";

        private const string Title = "Notify when property changes.";
        private const string MessageFormat = "Notify that property '{0}' changes.";
        private const string Description = "Notify when property changes.";
        private static readonly string HelpLink = WpfAnalyzers.HelpLink.ForId(DiagnosticId);

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            AnalyzerCategory.PropertyChanged,
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
            context.RegisterSyntaxNodeAction(HandleAssignment, SyntaxKind.SimpleAssignmentExpression);
        }

        private static void HandleAssignment(SyntaxNodeAnalysisContext context)
        {
            var assignment = (AssignmentExpressionSyntax)context.Node;
            if (assignment?.IsMissing != false ||
                assignment.FirstAncestorOrSelf<ConstructorConstraintSyntax>() != null ||
                assignment.FirstAncestorOrSelf<InitializerExpressionSyntax>() != null)
            {
                return;
            }

            var block = assignment.FirstAncestorOrSelf<BlockSyntax>();
            if (block == null)
            {
                return;
            }

            var typeDeclaration = assignment.FirstAncestorOrSelf<TypeDeclarationSyntax>();
            var typeSymbol = context.SemanticModel.GetDeclaredSymbolSafe(typeDeclaration, context.CancellationToken);
            if (!typeSymbol.Is(KnownSymbol.INotifyPropertyChanged))
            {
                return;
            }

            var field = context.SemanticModel.GetSymbolSafe(assignment.Left, context.CancellationToken) as IFieldSymbol;
            if (field == null || !typeSymbol.Equals(field.ContainingType))
            {
                return;
            }

            foreach (var member in typeDeclaration.Members)
            {
                var propertyDeclaration = member as PropertyDeclarationSyntax;
                if (propertyDeclaration == null)
                {
                    continue;
                }

                var property = context.SemanticModel.GetDeclaredSymbolSafe(propertyDeclaration, context.CancellationToken);
                var getter = Getter(propertyDeclaration);
                if (getter == null || property == null)
                {
                    continue;
                }

                using (var pooled = IdentifierNameWalker.Create(getter))
                {
                    foreach (var identifierName in pooled.Item.IdentifierNames)
                    {
                        var component = context.SemanticModel.GetSymbolSafe(identifierName, context.CancellationToken);
                        var componentField = component as IFieldSymbol;
                        if (componentField == null)
                        {
                            var propertySymbol = component as IPropertySymbol;
                            if (propertySymbol == null)
                            {
                                continue;
                            }

                            if (!Property.TryGetBackingField(propertySymbol, context.SemanticModel, context.CancellationToken, out componentField))
                            {
                                continue;
                            }
                        }

                        if (!field.Equals(componentField))
                        {
                            continue;
                        }

                        if (PropertyChanged.InvokesPropertyChangedFor(assignment, property, context.SemanticModel, context.CancellationToken) == PropertyChanged.InvokesPropertyChanged.No)
                        {
                            var properties = ImmutableDictionary.CreateRange(new[] { new KeyValuePair<string, string>(PropertyNameKey, property.Name), });
                            context.ReportDiagnostic(Diagnostic.Create(Descriptor, assignment.GetLocation(), properties, property.Name));
                        }
                    }
                }
            }
        }

        private static SyntaxNode Getter(PropertyDeclarationSyntax property)
        {
            if (property.ExpressionBody != null)
            {
                return property.ExpressionBody.Expression;
            }

            AccessorDeclarationSyntax getter;
            if (property.TryGetGetAccessorDeclaration(out getter))
            {
                return getter.Body;
            }

            return null;
        }
    }
}