namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WA1200FieldNameMustMatchRegisteredName : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WA1200";
        private const string Title = "DependencyProperty field must match registered name.";
        private const string MessageFormat = "DependencyProperty '{0}' field must be named {1}Property";
        private const string Description = Title;
        private const string HelpLink = "http://stackoverflow.com/";

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
            context.RegisterSyntaxNodeAction(HandleFieldDeclaration, SyntaxKind.FieldDeclaration);
        }

        private static void HandleFieldDeclaration(SyntaxNodeAnalysisContext context)
        {
            var fieldDeclaration = context.Node as FieldDeclarationSyntax;
            if (fieldDeclaration == null || fieldDeclaration.IsMissing || !fieldDeclaration.IsDependencyPropertyType())
            {
                return;
            }

            var registeredName = fieldDeclaration.DependencyPropertyRegisteredName();
            if (registeredName == null)
            {
                return;
            }

            var fieldName = fieldDeclaration.Name();
            if (!IsMatch(fieldName, registeredName))
            {
                var identifier = fieldDeclaration.Declaration.Variables.First().Identifier;
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, identifier.GetLocation(), fieldName, registeredName));
            }
        }

        private static bool IsMatch(string name, string registeredName)
        {
            const string Suffix = "Property";
            if (name.Length != registeredName.Length + Suffix.Length)
            {
                return false;
            }

            return name.StartsWith(registeredName) && name.EndsWith(Suffix);
        }
    }
}
