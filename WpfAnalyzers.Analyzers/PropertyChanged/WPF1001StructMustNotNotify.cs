namespace WpfAnalyzers.PropertyChanged
{
    using System.Collections.Immutable;
    using System.Linq;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    using WpfAnalyzers.PropertyChanged.Helpers;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF1001StructMustNotNotify : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF1001";
        private const string Title = "Struct must not implement INotifyPropertyChanged";
        private const string MessageFormat = "Struct '{0}' implements INotifyPropertyChanged";
        private const string Description = Title;
        private static readonly string HelpLink = WpfAnalyzers.HelpLink.ForId(DiagnosticId);

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
                                                                      DiagnosticId,
                                                                      Title,
                                                                      MessageFormat,
                                                                      AnalyzerCategory.PropertyChanged,
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
            context.RegisterSyntaxNodeAction(HandleDeclaration, SyntaxKind.StructDeclaration);
        }

        private static void HandleDeclaration(SyntaxNodeAnalysisContext context)
        {
            var structDeclaration = (StructDeclarationSyntax)context.Node;
            if (structDeclaration.ImplementsINotifyPropertyChanged())
            {
                var type = structDeclaration?.BaseList?.Types.First(x => x.IsINotifyPropertyChanged());
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, type?.GetLocation() ?? structDeclaration?.GetLocation(), context.ContainingSymbol.Name));
            }
        }
    }
}