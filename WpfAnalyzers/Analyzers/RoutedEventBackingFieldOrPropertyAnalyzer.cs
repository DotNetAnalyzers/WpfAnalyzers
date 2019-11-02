namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class RoutedEventBackingFieldOrPropertyAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Descriptors.WPF0100BackingFieldShouldMatchRegisteredName,
            Descriptors.WPF0101RegisterContainingTypeAsOwner,
            Descriptors.WPF0107BackingMemberShouldBeStaticReadonly,
            Descriptors.WPF0108DocumentRoutedEventBackingMember,
            Descriptors.WPF0150UseNameofInsteadOfLiteral,
            Descriptors.WPF0151UseNameofInsteadOfConstant);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(x => Handle(x), SyntaxKind.FieldDeclaration, SyntaxKind.PropertyDeclaration);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is MemberDeclarationSyntax memberDeclaration &&
                FieldOrProperty.TryCreate(context.ContainingSymbol, out var fieldOrProperty) &&
                fieldOrProperty.Type == KnownSymbols.RoutedEvent)
            {
                if (RoutedEvent.TryGetRegisteredName(fieldOrProperty, context.SemanticModel, context.CancellationToken, out var nameArg, out var registeredName))
                {
                    if (!fieldOrProperty.Name.IsParts(registeredName, "Event"))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.WPF0100BackingFieldShouldMatchRegisteredName,
                                FindIdentifier(context.Node).GetLocation(),
                                ImmutableDictionary<string, string>.Empty.Add("ExpectedName", registeredName + "Event"),
                                fieldOrProperty.Name,
                                registeredName));
                    }

                    if (fieldOrProperty.ContainingType.TryFindEvent(registeredName, out var eventSymbol))
                    {
                        if (nameArg.Expression is LiteralExpressionSyntax)
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    Descriptors.WPF0150UseNameofInsteadOfLiteral,
                                    nameArg.GetLocation(),
                                    ImmutableDictionary<string, string>.Empty.Add(nameof(IdentifierNameSyntax), eventSymbol.Name),
                                    eventSymbol.Name));
                        }
                        else if (!nameArg.Expression.IsNameof())
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    Descriptors.WPF0151UseNameofInsteadOfConstant,
                                    nameArg.GetLocation(),
                                    ImmutableDictionary<string, string>.Empty.Add(nameof(IdentifierNameSyntax), eventSymbol.Name),
                                    eventSymbol.Name));
                        }
                    }

                    if (context.ContainingSymbol.ContainingType.TryFindEvent(registeredName, out _) &&
                        context.ContainingSymbol.DeclaredAccessibility.IsEither(Accessibility.Protected, Accessibility.Internal, Accessibility.Public) &&
                        !HasStandardText(memberDeclaration, registeredName, out var comment))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.WPF0108DocumentRoutedEventBackingMember,
                                comment == null
                                    ? BackingFieldOrProperty.FindIdentifier(memberDeclaration).GetLocation()
                                    : comment.GetLocation(),
                                properties: ImmutableDictionary<string, string>.Empty.Add(nameof(CrefParameterSyntax), registeredName)));
                    }
                }

                if (RoutedEvent.TryGetRegisteredType(fieldOrProperty, context.SemanticModel, context.CancellationToken, out var typeArg, out var registeredOwnerType) &&
                    !Equals(registeredOwnerType, context.ContainingSymbol.ContainingType))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptors.WPF0101RegisterContainingTypeAsOwner,
                            typeArg.GetLocation(),
                            fieldOrProperty.ContainingType.Name,
                            registeredName));
                }

                if (!fieldOrProperty.IsStaticReadOnly())
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptors.WPF0107BackingMemberShouldBeStaticReadonly,
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

        private static bool HasStandardText(MemberDeclarationSyntax memberDeclaration, string name, [NotNullWhen(true)] out DocumentationCommentTriviaSyntax? comment)
        {
            return memberDeclaration.TryGetDocumentationComment(out comment) &&
                   comment.TryGetSummary(out var summary) &&
                   summary.ToString().IsParts("<summary>Identifies the <see cref=\"", name, "\"/> routed event.</summary>");
        }
    }
}
