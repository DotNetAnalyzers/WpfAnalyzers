namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class RoutedEventBackingFieldOrPropertyAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            WPF0100BackingFieldShouldMatchRegisteredName.Descriptor,
            WPF0101RegisterContainingTypeAsOwner.Descriptor,
            WPF0107BackingMemberShouldBeStaticReadonly.Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Handle, SyntaxKind.FieldDeclaration, SyntaxKind.PropertyDeclaration);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.Node is MemberDeclarationSyntax memberDeclaration &&
                FieldOrProperty.TryCreate(context.ContainingSymbol, out var fieldOrProperty) &&
                fieldOrProperty.Type == KnownSymbol.RoutedEvent)
            {
                if (RoutedEvent.TryGetRegisteredName(fieldOrProperty, context.SemanticModel, context.CancellationToken, out var registeredName) &&
                    !fieldOrProperty.Name.IsParts(registeredName, "Event"))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            WPF0100BackingFieldShouldMatchRegisteredName.Descriptor,
                            FindIdentifier(context.Node).GetLocation(),
                            ImmutableDictionary<string, string>.Empty.Add("ExpectedName", registeredName + "Event"),
                            fieldOrProperty.Name,
                            registeredName));
                }

                if (RoutedEvent.TryGetRegisteredType(fieldOrProperty, context.SemanticModel, context.CancellationToken, out var typeArg, out var registeredOwnerType) &&
                    !Equals(registeredOwnerType, context.ContainingSymbol.ContainingType))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            WPF0101RegisterContainingTypeAsOwner.Descriptor,
                            typeArg.GetLocation(),
                            fieldOrProperty.ContainingType.Name,
                            registeredName));
                }

                if (!fieldOrProperty.IsStaticReadOnly())
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            WPF0107BackingMemberShouldBeStaticReadonly.Descriptor,
                            BackingFieldOrProperty.FindIdentifier(memberDeclaration).GetLocation()));
                }
            }
        }

        private static SyntaxToken FindIdentifier(SyntaxNode node)
        {
            if (node is PropertyDeclarationSyntax propertyDeclaration)
            {
                return propertyDeclaration.Identifier;
            }

            if (node is FieldDeclarationSyntax fieldDeclaration)
            {
                if (fieldDeclaration.Declaration.Variables.TrySingle(out var variable))
                {
                    return variable.Identifier;
                }
            }

            return node.GetFirstToken();
        }
    }
}
