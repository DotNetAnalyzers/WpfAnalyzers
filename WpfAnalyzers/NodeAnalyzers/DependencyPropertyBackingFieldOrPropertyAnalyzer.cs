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
            WPF0001BackingFieldShouldMatchRegisteredName.Descriptor,
            WPF0002BackingFieldShouldMatchRegisteredName.Descriptor,
            WPF0060DocumentDependencyPropertyBackingMember.Descriptor,
            WPF0030BackingFieldShouldBeStaticReadonly.Descriptor,
            WPF0031FieldOrder.Descriptor);

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

            if (context.Node is MemberDeclarationSyntax memberDeclaration)
            {
                if (BackingFieldOrProperty.TryCreateCandidate(context.ContainingSymbol, out var candidate) &&
                         DependencyProperty.TryGetRegisterInvocationRecursive(candidate, context.SemanticModel, context.CancellationToken, out var registerInvocation, out _))
                {
                    if (BackingFieldOrProperty.TryCreateForDependencyProperty(context.ContainingSymbol, out var backingMember))
                    {
                        if (registerInvocation.TryGetArgumentAtIndex(0, out var nameArg) &&
                            nameArg.TryGetStringValue(context.SemanticModel, context.CancellationToken, out var registeredName))
                        {
                            if (backingMember.Type == KnownSymbol.DependencyProperty &&
                                !backingMember.Name.IsParts(registeredName, "Property"))
                            {
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        WPF0001BackingFieldShouldMatchRegisteredName.Descriptor,
                                        BackingFieldOrProperty.FindIdentifier(memberDeclaration)
                                                              .GetLocation(),
                                        ImmutableDictionary<string, string>.Empty.Add("ExpectedName", registeredName + "Property"),
                                        backingMember.Name,
                                        registeredName));
                            }

                            if (backingMember.Type == KnownSymbol.DependencyPropertyKey &&
                                !backingMember.Name.IsParts(registeredName, "PropertyKey"))
                            {
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        WPF0002BackingFieldShouldMatchRegisteredName.Descriptor,
                                        BackingFieldOrProperty.FindIdentifier(memberDeclaration)
                                                              .GetLocation(),
                                        ImmutableDictionary<string, string>.Empty.Add("ExpectedName", registeredName + "PropertyKey"),
                                        backingMember.Name,
                                        registeredName));
                            }

                            if (context.ContainingSymbol.DeclaredAccessibility.IsEither(
                                    Accessibility.Protected, Accessibility.Internal, Accessibility.Public) &&
                                context.ContainingSymbol.ContainingType.TryFindProperty(registeredName, out _) &&
                                !HasStandardText(memberDeclaration, registeredName, out var comment))
                            {
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        WPF0060DocumentDependencyPropertyBackingMember.Descriptor,
                                        comment == null
                                            ? BackingFieldOrProperty.FindIdentifier(memberDeclaration)
                                                                    .GetLocation()
                                            : comment.GetLocation()));
                            }
                        }
                        else if (DependencyProperty.TryGetPropertyByName(backingMember, out var property))
                        {
                            if (backingMember.Type == KnownSymbol.DependencyProperty &&
                                !backingMember.Name.IsParts(property.Name, "Property"))
                            {
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        WPF0001BackingFieldShouldMatchRegisteredName.Descriptor,
                                        BackingFieldOrProperty.FindIdentifier(memberDeclaration)
                                                              .GetLocation(),
                                        backingMember.Name,
                                        property.Name));
                            }

                            if (backingMember.Type == KnownSymbol.DependencyPropertyKey &&
                                !backingMember.Name.IsParts(property.Name, "PropertyKey"))
                            {
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        WPF0002BackingFieldShouldMatchRegisteredName.Descriptor,
                                        BackingFieldOrProperty.FindIdentifier(memberDeclaration)
                                                              .GetLocation(),
                                        backingMember.Name,
                                        property.Name));
                            }
                        }

                        if (context.Node is FieldDeclarationSyntax fieldDeclaration &&
                            DependencyProperty.TryGetDependencyPropertyKeyField(
                                backingMember, context.SemanticModel, context.CancellationToken, out var keyField) &&
                            backingMember.ContainingType == keyField.ContainingType &&
                            keyField.TryGetSyntaxReference(out var reference))
                        {
                            var keyNode = reference.GetSyntax(context.CancellationToken);
                            if (ReferenceEquals(fieldDeclaration.SyntaxTree, keyNode.SyntaxTree) &&
                                fieldDeclaration.SpanStart < keyNode.SpanStart)
                            {
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        WPF0031FieldOrder.Descriptor,
                                        fieldDeclaration.GetLocation(),
                                        keyField.Name,
                                        backingMember.Name));
                            }
                        }
                    }

                    if (!candidate.FieldOrProperty.IsStaticReadOnly())
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                WPF0030BackingFieldShouldBeStaticReadonly.Descriptor,
                                BackingFieldOrProperty.FindIdentifier(memberDeclaration).GetLocation(),
                                candidate.Name,
                                candidate.Type.Name));
                    }
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
