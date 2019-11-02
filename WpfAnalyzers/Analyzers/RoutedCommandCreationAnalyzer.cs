namespace WpfAnalyzers
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
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
            Descriptors.WPF0120RegisterContainingMemberAsNameForRoutedCommand,
            Descriptors.WPF0121RegisterContainingTypeAsOwnerForRoutedCommand,
            Descriptors.WPF0122RegisterRoutedCommand,
            Descriptors.WPF0123BackingMemberShouldBeStaticReadonly,
            Descriptors.WPF0150UseNameofInsteadOfLiteral,
            Descriptors.WPF0151UseNameofInsteadOfConstant);

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
                (objectCreation.Type == KnownSymbols.RoutedCommand || objectCreation.Type == KnownSymbols.RoutedUICommand) &&
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
                                Descriptors.WPF0121RegisterContainingTypeAsOwnerForRoutedCommand,
                                ownerTypeArg.GetLocation(),
                                context.ContainingSymbol.ContainingType.ToMinimalDisplayString(context.SemanticModel, objectCreation.SpanStart)));
                    }
                }

                if (TryGetBackingMember(objectCreation, context, out var fieldOrProperty, out var memberDeclaration))
                {
                    if (ctor.TryFindParameter("name", out var nameParameter))
                    {
                        if (objectCreation.TryFindArgument(nameParameter, out var nameArg) &&
                            nameArg.TryGetStringValue(context.SemanticModel, context.CancellationToken, out var registeredName))
                        {
                            if (registeredName != fieldOrProperty.Name)
                            {
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        Descriptors.WPF0120RegisterContainingMemberAsNameForRoutedCommand,
                                        nameArg.GetLocation(),
                                        ImmutableDictionary<string, string>.Empty.Add(nameof(IdentifierNameSyntax), fieldOrProperty.Name),
                                        fieldOrProperty.Name));
                            }
                            else if (nameArg.Expression.IsKind(SyntaxKind.StringLiteralExpression))
                            {
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        Descriptors.WPF0150UseNameofInsteadOfLiteral,
                                        nameArg.GetLocation(),
                                        ImmutableDictionary<string, string>.Empty.Add(nameof(IdentifierNameSyntax), fieldOrProperty.Name),
                                        fieldOrProperty.Name));
                            }
                            else if (!nameArg.Expression.IsNameof())
                            {
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        Descriptors.WPF0151UseNameofInsteadOfConstant,
                                        nameArg.GetLocation(),
                                        ImmutableDictionary<string, string>.Empty.Add(nameof(IdentifierNameSyntax), fieldOrProperty.Name),
                                        fieldOrProperty.Name));
                            }
                        }
                    }
                    else
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.WPF0122RegisterRoutedCommand,
                                objectCreation.ArgumentList.GetLocation(),
                                ImmutableDictionary.CreateRange(new[]
                                {
                                    new KeyValuePair<string, string>(nameof(IdentifierNameSyntax), fieldOrProperty.Name),
                                    new KeyValuePair<string, string>(nameof(TypeOfExpressionSyntax), context.ContainingSymbol.ContainingType.ToMinimalDisplayString(context.SemanticModel, objectCreation.SpanStart)),
                                })));
                    }

                    if (!fieldOrProperty.IsStaticReadOnly())
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0123BackingMemberShouldBeStaticReadonly, BackingFieldOrProperty.FindIdentifier(memberDeclaration).GetLocation()));
                    }
                }
            }
        }

        private static bool TryGetBackingMember(ObjectCreationExpressionSyntax objectCreation, SyntaxNodeAnalysisContext context, out FieldOrProperty fieldOrProperty, [NotNullWhen(true)] out MemberDeclarationSyntax? memberDeclaration)
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
