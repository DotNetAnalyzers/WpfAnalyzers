namespace WpfAnalyzers.PropertyChanged
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using WpfAnalyzers.PropertyChanged.Helpers;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF1011ImplementINotifyPropertyChanged : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF1011";
        private const string Title = "Implement INotifyPropertyChanged.";
        private const string MessageFormat = "Implement INotifyPropertyChanged.";
        private const string Description = "Implement INotifyPropertyChanged.";
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
            context.RegisterSyntaxNodeAction(HandlePropertyDeclaration, SyntaxKind.PropertyDeclaration);
            context.RegisterSyntaxNodeAction(HandleEventFieldDeclaration, SyntaxKind.EventFieldDeclaration);
        }

        private static void HandlePropertyDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            var propertySymbol = (IPropertySymbol)context.ContainingSymbol;
            if (propertySymbol.ContainingType.Is(KnownSymbol.INotifyPropertyChanged) ||
                propertySymbol.ContainingType.Is(KnownSymbol.MarkupExtension) ||
                propertySymbol.ContainingType.Is(KnownSymbol.IValueConverter) ||
                propertySymbol.ContainingType.Is(KnownSymbol.IMultiValueConverter) ||
                propertySymbol.ContainingType.Is(KnownSymbol.DataTemplateSelector) ||
                propertySymbol.ContainingType.Is(KnownSymbol.DependencyObject))
            {
                return;
            }

            var declaration = (PropertyDeclarationSyntax)context.Node;
            if (Property.ShouldNotify(declaration, propertySymbol, context.SemanticModel, context.CancellationToken))
            {
                if (HasMemberNamedPropertyChanged(propertySymbol.ContainingType))
                {
                    return;
                }

                context.ReportDiagnostic(Diagnostic.Create(Descriptor, declaration.GetLocation(), context.ContainingSymbol.Name));
            }
        }

        private static void HandleEventFieldDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            var eventSymbol = (IEventSymbol)context.ContainingSymbol;
            if (eventSymbol.Name != KnownSymbol.INotifyPropertyChanged.PropertyChanged.Name ||
                eventSymbol.Type != KnownSymbol.PropertyChangedEventHandler)
            {
                return;
            }

            if (!eventSymbol.ContainingType.Is(KnownSymbol.INotifyPropertyChanged))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, context.Node.FirstAncestorOrSelf<EventFieldDeclarationSyntax>().GetLocation(), context.ContainingSymbol.Name));
            }
        }

        private static bool HasMemberNamedPropertyChanged(ITypeSymbol type)
        {
            return type.TryGetSingleMember("PropertyChanged", out ISymbol member);
        }
    }
}