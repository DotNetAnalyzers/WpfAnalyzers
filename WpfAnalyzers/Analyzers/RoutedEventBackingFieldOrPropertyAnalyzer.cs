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
            Descriptors.WPF0100BackingFieldShouldMatchRegisteredName,
            Descriptors.WPF0101RegisterContainingTypeAsOwner,
            Descriptors.WPF0107BackingMemberShouldBeStaticReadonly,
            Descriptors.WPF0108DocumentRoutedEventBackingMember,
            Descriptors.WPF0150UseNameofInsteadOfLiteral,
            Descriptors.WPF0151UseNameofInsteadOfConstant);

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
                FieldOrProperty.TryCreate(context.ContainingSymbol, out var backing) &&
                backing.Type == KnownSymbols.RoutedEvent)
            {
                if (RoutedEvent.TryGetRegisteredName(backing, context.SemanticModel, context.CancellationToken, out var nameArg, out var registeredName))
                {
                    if (!backing.Name.IsParts(registeredName, "Event"))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.WPF0100BackingFieldShouldMatchRegisteredName,
                                backing.Symbol.Locations[0],
                                ImmutableDictionary<string, string?>.Empty.Add("ExpectedName", registeredName + "Event"),
                                backing.Name,
                                registeredName));
                    }

                    if (backing.ContainingType.TryFindEvent(registeredName, out var eventSymbol))
                    {
                        if (nameArg.Expression is LiteralExpressionSyntax)
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    Descriptors.WPF0150UseNameofInsteadOfLiteral,
                                    nameArg.GetLocation(),
                                    ImmutableDictionary<string, string?>.Empty.Add(nameof(IdentifierNameSyntax), eventSymbol.Name),
                                    eventSymbol.Name));
                        }
                        else if (!nameArg.Expression.IsNameof())
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    Descriptors.WPF0151UseNameofInsteadOfConstant,
                                    nameArg.GetLocation(),
                                    ImmutableDictionary<string, string?>.Empty.Add(nameof(IdentifierNameSyntax), eventSymbol.Name),
                                    eventSymbol.Name));
                        }
                    }

                    if (context.ContainingSymbol.ContainingType.TryFindEvent(registeredName, out _) &&
                        context.ContainingSymbol.DeclaredAccessibility.IsEither(Accessibility.Protected, Accessibility.Internal, Accessibility.Public))
                    {
                        var summaryFormat = "<summary>Identifies the <see cref=\"{registered_name}\"/> routed event.</summary>";
                        if (memberDeclaration.TryGetDocumentationComment(out var comment))
                        {
                            if (comment.VerifySummary(summaryFormat, registeredName) is { } summaryError)
                            {
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        Descriptors.WPF0108DocumentRoutedEventBackingMember,
                                        summaryError.Location,
                                        ImmutableDictionary<string, string?>.Empty.Add(nameof(DocComment), summaryError.Text)));
                            }
                        }
                        else
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    Descriptors.WPF0108DocumentRoutedEventBackingMember,
                                    backing.Symbol.Locations[0],
                                    ImmutableDictionary<string, string?>.Empty.Add(
                                        nameof(DocComment),
                                        $"/// {DocComment.Format(summaryFormat, registeredName)}")));
                        }
                    }
                }

                if (RoutedEvent.TryGetRegisteredType(backing, context.SemanticModel, context.CancellationToken, out var typeArg, out var registeredOwnerType) &&
                    !Equals(registeredOwnerType, context.ContainingSymbol.ContainingType))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptors.WPF0101RegisterContainingTypeAsOwner,
                            typeArg.GetLocation(),
                            backing.ContainingType.Name,
                            registeredName));
                }

                if (!backing.IsStaticReadOnly())
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptors.WPF0107BackingMemberShouldBeStaticReadonly,
                            BackingFieldOrProperty.FindIdentifier(memberDeclaration).GetLocation()));
                }
            }
        }
    }
}
