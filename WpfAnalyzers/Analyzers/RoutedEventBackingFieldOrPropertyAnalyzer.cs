namespace WpfAnalyzers;

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
            context.ContainingSymbol is { } &&
            FieldOrProperty.TryCreate(context.ContainingSymbol, out var backing) &&
            backing.Type == KnownSymbols.RoutedEvent &&
            backing.Value(context.CancellationToken) is InvocationExpressionSyntax invocation &&
            EventManager.RegisterRoutedEvent.Match(invocation, context.SemanticModel, context.CancellationToken) is { NameArgument: { } nameArgument, OwnerTypeArgument: { } ownerTypeArgument })
        {
            if (nameArgument.TryGetStringValue(context.SemanticModel, context.CancellationToken, out var registeredName) &&
                registeredName is { })
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
                    if (nameArgument.Expression is LiteralExpressionSyntax)
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.WPF0150UseNameofInsteadOfLiteral,
                                nameArgument.GetLocation(),
                                ImmutableDictionary<string, string?>.Empty.Add(nameof(IdentifierNameSyntax), eventSymbol.Name),
                                eventSymbol.Name));
                    }
                    else if (!nameArgument.Expression.IsNameof())
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.WPF0151UseNameofInsteadOfConstant,
                                nameArgument.GetLocation(),
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

            if (ownerTypeArgument is { Expression: TypeOfExpressionSyntax { Type: { } type } } &&
                context.SemanticModel.GetType(type, context.CancellationToken) is { } registeredOwnerType &&
                !TypeSymbolComparer.Equal(registeredOwnerType, context.ContainingSymbol.ContainingType))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptors.WPF0101RegisterContainingTypeAsOwner,
                        type.GetLocation(),
                        backing.ContainingType.Name));
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
