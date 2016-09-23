//namespace WpfAnalyzers.DependencyProperties
//{
//    using System.Collections.Immutable;
//    using Microsoft.CodeAnalysis;
//    using Microsoft.CodeAnalysis.CSharp;
//    using Microsoft.CodeAnalysis.Diagnostics;

//    using WpfAnalyzers.DependencyProperties.Internals;

//    /// <summary>
//    /// DependencyProperty field must be named &lt;Name&gt;Property
//    /// </summary>
//    [DiagnosticAnalyzer(LanguageNames.CSharp)]
//    internal class WA1200DependencyPropertyFieldMustBeNamedNameProperty : DiagnosticAnalyzer
//    {
//        public const string DiagnosticId = "WA1200";
//        private const string Title = "DependencyProperty field must be named <Name>Property";
//        private const string MessageFormat = "DependencyProperty '{0}' field must be named <Name>Property";
//        private const string Description = "DependencyProperty field must be named <Name>Property";
//        private const string HelpLink = "http://stackoverflow.com/";

//        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
//            DiagnosticId,
//            Title,
//            MessageFormat,
//            AnalyzerCategory.DependencyProperties,
//            DiagnosticSeverity.Warning,
//            AnalyzerConstants.EnabledByDefault,
//            Description,
//            HelpLink);

//        /// <inheritdoc/>
//        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

//        /// <inheritdoc/>
//        public override void Initialize(AnalysisContext context)
//        {
//            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
//            context.EnableConcurrentExecution();
//            context.RegisterSyntaxNodeAction(HandleFieldDeclaration, SyntaxKind.FieldDeclaration);
//        }

//        private static void HandleFieldDeclaration(SyntaxNodeAnalysisContext context)
//        {
//            var fieldSymbol = (IFieldSymbol)context.ContainingSymbol;
//            if (!fieldSymbol.IsDependencyPropertyField())
//            {
//                return;
//            }

//            //context.ReportDiagnostic(Diagnostic.Create(Descriptor, context.Node.GetLocation(), fieldSymbol.Name));
//        }
//    }
//}
