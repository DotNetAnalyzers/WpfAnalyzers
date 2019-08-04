namespace WpfAnalyzers
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class RoutedCommandCreationAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            WPF0120RegisterContainingMemberAsNameForRoutedCommand.Descriptor,
            WPF0121RegisterContainingTypeAsOwnerForRoutedCommand.Descriptor,
            WPF0122RegisterRoutedCommand.Descriptor,
            WPF0123BackingMemberShouldBeStaticReadonly.Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(x => Handle(x), SyntaxKind.ObjectCreationExpression);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is ObjectCreationExpressionSyntax objectCreation &&
                (objectCreation.Type == KnownSymbol.RoutedCommand || objectCreation.Type == KnownSymbol.RoutedUICommand) &&
                context.SemanticModel.TryGetSymbol(objectCreation, context.CancellationToken, out var ctor))
            {
                if (ctor.TryFindParameter("ownerType", out var parameter))
                {
                    if (objectCreation.TryFindArgument(parameter, out var ownerTypeArg) &&
                        ownerTypeArg.TryGetTypeofValue(context.SemanticModel, context.CancellationToken, out var type) &&
                        !type.Equals(context.ContainingSymbol.ContainingType))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                WPF0121RegisterContainingTypeAsOwnerForRoutedCommand.Descriptor,
                                ownerTypeArg.GetLocation(),
                                context.ContainingSymbol.ContainingType.ToMinimalDisplayString(context.SemanticModel, objectCreation.SpanStart)));
                    }
                }

                if (TryGetBackingMember(objectCreation, context, out var fieldOrProperty, out var memberDeclaration))
                {
                    if (ctor.TryFindParameter("name", out var nameParameter))
                    {
                        if (objectCreation.TryFindArgument(nameParameter, out var nameArg) &&
                            nameArg.TryGetStringValue(context.SemanticModel, context.CancellationToken, out var registeredName) &&
                            registeredName != fieldOrProperty.Name)
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    WPF0120RegisterContainingMemberAsNameForRoutedCommand.Descriptor,
                                    nameArg.GetLocation(),
                                    ImmutableDictionary<string, string>.Empty.Add(nameof(IdentifierNameSyntax), fieldOrProperty.Name),
                                    fieldOrProperty.Name));
                        }
                    }
                    else
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                WPF0122RegisterRoutedCommand.Descriptor,
                                objectCreation.ArgumentList.GetLocation(),
                                ImmutableDictionary.CreateRange(new[]
                                {
                                    new KeyValuePair<string, string>(nameof(IdentifierNameSyntax), fieldOrProperty.Name),
                                    new KeyValuePair<string, string>(nameof(TypeOfExpressionSyntax), context.ContainingSymbol.ContainingType.ToMinimalDisplayString(context.SemanticModel, objectCreation.SpanStart)),
                                })));
                    }

                    if (!fieldOrProperty.IsStaticReadOnly())
                    {
                        context.ReportDiagnostic(Diagnostic.Create(WPF0123BackingMemberShouldBeStaticReadonly.Descriptor, BackingFieldOrProperty.FindIdentifier(memberDeclaration).GetLocation()));
                    }
                }
            }
        }

        private static bool TryGetBackingMember(ObjectCreationExpressionSyntax objectCreation, SyntaxNodeAnalysisContext context, out FieldOrProperty fieldOrProperty, out MemberDeclarationSyntax memberDeclaration)
        {
            fieldOrProperty = default;
            memberDeclaration = null;
            switch (objectCreation.Parent)
            {
                case EqualsValueClauseSyntax _:
                    return objectCreation.TryFirstAncestor(out memberDeclaration) &&
                           FieldOrProperty.TryCreate(context.ContainingSymbol, out fieldOrProperty);
                case ArrowExpressionClauseSyntax _:
                    return objectCreation.TryFirstAncestor(out memberDeclaration) &&
                           context.ContainingSymbol is IMethodSymbol getter &&
                           FieldOrProperty.TryCreate(getter.AssociatedSymbol, out fieldOrProperty);
            }

            return false;
        }
    }
}
