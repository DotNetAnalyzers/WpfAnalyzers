namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
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
            Descriptors.WPF0031FieldOrder,
            Descriptors.WPF0176StyleTypedPropertyMissing);

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
                        if (backingMember.Type == KnownSymbols.DependencyProperty &&
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

                        if (backingMember.Type == KnownSymbols.DependencyPropertyKey &&
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

                        if (DependencyProperty.TryGetRegisteredType(backingMember, context.SemanticModel, context.CancellationToken, out var type) &&
                            type.Is(KnownSymbols.Style) &&
                            !TryFindStyleTypedPropertyAttribute(memberDeclaration, registeredName, context.SemanticModel, context.CancellationToken) &&
                            backingMember.FieldOrProperty.Symbol.DeclaredAccessibility.IsEither(Accessibility.Public, Accessibility.Internal))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    Descriptors.WPF0176StyleTypedPropertyMissing,
                                    BackingFieldOrProperty.FindIdentifier(memberDeclaration).GetLocation(),
                                    ImmutableDictionary<string, string>.Empty.Add(
                                        nameof(AttributeListSyntax),
                                        $"[StyleTypedProperty(Property = {(context.ContainingSymbol.ContainingType.TryFindProperty(registeredName, out _) ? $"nameof({registeredName})" : $"\"{registeredName}\"")}, StyleTargetType = typeof(TYPE))]"),
                                    backingMember.Name));
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

        private static bool TryFindStyleTypedPropertyAttribute(MemberDeclarationSyntax memberDeclaration, string registeredName, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (memberDeclaration.Parent is TypeDeclarationSyntax containingType)
            {
                foreach (var list in containingType.AttributeLists)
                {
                    foreach (var candidate in list.Attributes)
                    {
                        if (semanticModel.TryGetNamedType(candidate, KnownSymbols.StyleTypedPropertyAttribute, cancellationToken, out _) &&
                            candidate.TryFindArgument(0, "Property", out var argument) &&
                            semanticModel.TryGetConstantValue(argument.Expression, cancellationToken, out string? text) &&
                            text == registeredName)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static bool HasStandardText(MemberDeclarationSyntax memberDeclaration, string name, [NotNullWhen(true)] out DocumentationCommentTriviaSyntax? comment)
        {
            return memberDeclaration.TryGetDocumentationComment(out comment) &&
                   comment.TryGetSummary(out var summary) &&
                   summary.ToString().IsParts("<summary>Identifies the <see cref=\"", name, "\"/> dependency property.</summary>");
        }
    }
}
