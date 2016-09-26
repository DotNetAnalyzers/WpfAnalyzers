namespace WpfAnalyzers.PropertyChanged
{
    using System;
    using System.Collections.Immutable;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// DependencyProperty field must be static readonly.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WA1101StructMustNotImplementINotifyPropertyChanged : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WA1101";
        private const string Title = "Struct must not implement INotifyPropertyChanged";
        private const string MessageFormat = "Struct '{0}' implements INotifyPropertyChanged";
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
            context.RegisterSyntaxNodeAction(HandleDeclaration, SyntaxKind.StructDeclaration);
        }

        private static void HandleDeclaration(SyntaxNodeAnalysisContext context)
        {
            var structDeclaration = (StructDeclarationSyntax)context.Node;
            var types = structDeclaration?.BaseList?.Types;
            if (types == null)
            {
                return;
            }

            foreach (var type in types.Value)
            {
                if ((type.Type as IdentifierNameSyntax)?.Identifier.Text.EndsWith("INotifyPropertyChanged") == true)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, type.GetLocation(), context.ContainingSymbol.Name));
                }
            }
        }
    }
}