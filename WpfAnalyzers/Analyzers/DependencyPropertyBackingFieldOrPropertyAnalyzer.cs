namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class DependencyPropertyBackingFieldOrPropertyAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Descriptors.WPF0001BackingFieldShouldMatchRegisteredName,
            Descriptors.WPF0002BackingFieldShouldMatchRegisteredName,
            Descriptors.WPF0060DocumentDependencyPropertyBackingMember,
            Descriptors.WPF0030BackingFieldShouldBeStaticReadonly,
            Descriptors.WPF0031FieldOrder);

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
                context.Node is MemberDeclarationSyntax memberDeclaration)
            {
                if (BackingFieldOrProperty.TryCreateForDependencyProperty(context.ContainingSymbol, out var backingMember))
                {
                    if (DependencyProperty.TryGetRegisteredName(backingMember, context.SemanticModel, context.CancellationToken, out _, out var registeredName))
                    {
                        if (backingMember.Type == KnownSymbol.DependencyProperty &&
                            !backingMember.Name.IsParts(registeredName, "Property"))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    Descriptors.WPF0001BackingFieldShouldMatchRegisteredName,
                                    BackingFieldOrProperty.FindIdentifier(memberDeclaration).GetLocation(),
                                    ImmutableDictionary<string, string>.Empty.Add("ExpectedName", registeredName + "Property"),
                                    backingMember.Name,
                                    registeredName));
                        }

                        if (backingMember.Type == KnownSymbol.DependencyPropertyKey &&
                            !backingMember.Name.IsParts(registeredName, "PropertyKey"))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    Descriptors.WPF0002BackingFieldShouldMatchRegisteredName,
                                    BackingFieldOrProperty.FindIdentifier(memberDeclaration).GetLocation(),
                                    ImmutableDictionary<string, string>.Empty.Add("ExpectedName", registeredName + "PropertyKey"),
                                    backingMember.Name,
                                    registeredName));
                        }

                        if (context.ContainingSymbol.ContainingType.TryFindProperty(registeredName, out _) &&
                            context.ContainingSymbol.DeclaredAccessibility.IsEither(Accessibility.Protected, Accessibility.Internal, Accessibility.Public) &&
                            !HasStandardText(memberDeclaration, registeredName, out var comment))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    Descriptors.WPF0060DocumentDependencyPropertyBackingMember,
                                    comment == null
                                        ? BackingFieldOrProperty.FindIdentifier(memberDeclaration).GetLocation()
                                        : comment.GetLocation(),
                                    properties: ImmutableDictionary<string, string>.Empty.Add(nameof(CrefParameterSyntax), registeredName)));
                        }
                    }

                    if (DependencyProperty.TryGetDependencyPropertyKeyFieldOrProperty(backingMember, context.SemanticModel, context.CancellationToken, out var keyMember) &&
                        Equals(backingMember.ContainingType, keyMember.ContainingType) &&
                        keyMember.TryGetSyntaxReference(out var reference) &&
                        ReferenceEquals(reference.SyntaxTree, context.Node.SyntaxTree) &&
                        reference.Span.Start > context.Node.SpanStart)
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.WPF0031FieldOrder,
                                reference.GetSyntax(context.CancellationToken).GetLocation(),
                                additionalLocations: new[] { context.Node.GetLocation() },
                                keyMember.Name,
                                backingMember.Name));
                    }
                }

                if (BackingFieldOrProperty.TryCreateCandidate(context.ContainingSymbol, out var candidate) &&
                    DependencyProperty.TryGetRegisterInvocationRecursive(candidate, context.SemanticModel, context.CancellationToken, out _, out _) &&
                    !candidate.FieldOrProperty.IsStaticReadOnly())
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptors.WPF0030BackingFieldShouldBeStaticReadonly,
                            BackingFieldOrProperty.FindIdentifier(memberDeclaration).GetLocation(),
                            candidate.Name,
                            candidate.Type.Name));
                }
            }
        }

        private static bool HasStandardText(MemberDeclarationSyntax memberDeclaration, string name, out DocumentationCommentTriviaSyntax comment)
        {
            return memberDeclaration.TryGetDocumentationComment(out comment) &&
                   comment.TryGetSummary(out var summary) &&
                   summary.ToString().IsParts("<summary>Identifies the <see cref=\"", name, "\"/> dependency property.</summary>");
        }
    }
}
