namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0002BackingFieldForDependencyPropertyKeyShouldMatchRegisteredName : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0002";
        private const string Title = "Backing field for a DependencyPropertyKey should match registered name.";
        private const string MessageFormat = "Field '{0}' that is backing field for the DependencyPropertyKey registered as '{1}' must be named '{1}PropertyKey'";
        private const string Description = "A DependencyPropertyKey's backing field must be named with the name it is registered with suffixed by 'PropertyKey'";
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
            context.RegisterSyntaxNodeAction(HandleFieldDeclaration, SyntaxKind.FieldDeclaration);
        }

        private static void HandleFieldDeclaration(SyntaxNodeAnalysisContext context)
        {
            var fieldDeclaration = context.Node as FieldDeclarationSyntax;
            if (fieldDeclaration == null || fieldDeclaration.IsMissing || !fieldDeclaration.IsDependencyPropertyKeyType())
            {
                return;
            }

            string registeredName;
            if (!fieldDeclaration.TryGetDependencyPropertyRegisteredName(context.SemanticModel, out registeredName))
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
            const string suffix = "PropertyKey";
            if (name.Length != registeredName.Length + suffix.Length)
            {
                return false;
            }

            return name.StartsWith(registeredName) && name.EndsWith(suffix);
        }
    }
}