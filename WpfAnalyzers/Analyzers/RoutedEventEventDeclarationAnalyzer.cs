﻿namespace WpfAnalyzers;

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Gu.Roslyn.AnalyzerExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class RoutedEventEventDeclarationAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Descriptors.WPF0102EventDeclarationName,
        Descriptors.WPF0103EventDeclarationAddRemove,
        Descriptors.WPF0104EventDeclarationAddHandlerInAdd,
        Descriptors.WPF0105EventDeclarationRemoveHandlerInRemove,
        Descriptors.WPF0106EventDeclarationUseRegisteredHandlerType);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(x => Handle(x), SyntaxKind.EventDeclaration);
    }

    private static void Handle(SyntaxNodeAnalysisContext context)
    {
        if (!context.IsExcludedFromAnalysis() &&
            context is { ContainingSymbol: IEventSymbol eventSymbol, Node: EventDeclarationSyntax eventDeclaration } &&
            EventDeclarationWalker.TryGetCalls(eventDeclaration, out var addCall, out var removeCall))
        {
            if (addCall.TryGetMethodName(out var addName) &&
                addName != "AddHandler")
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptors.WPF0104EventDeclarationAddHandlerInAdd,
                        addCall.GetLocation()));
            }

            if (removeCall.TryGetMethodName(out var removeName) &&
                removeName != "RemoveHandler")
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptors.WPF0105EventDeclarationRemoveHandlerInRemove,
                        removeCall.GetLocation()));
            }

            if (addCall.TryGetArgumentAtIndex(0, out var addArg) &&
                addArg.Expression is IdentifierNameSyntax addIdentifier &&
                removeCall.TryGetArgumentAtIndex(0, out var removeArg) &&
                removeArg.Expression is IdentifierNameSyntax removeIdentifier)
            {
                if (addIdentifier.Identifier.ValueText != removeIdentifier.Identifier.ValueText)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptors.WPF0103EventDeclarationAddRemove,
                            eventDeclaration.Identifier.GetLocation(),
                            addIdentifier.Identifier.ValueText,
                            removeIdentifier.Identifier.ValueText));
                }
                else if (eventDeclaration.Parent is TypeDeclarationSyntax typeDeclaration &&
                         BackingFieldWalker.TryGetRegistration(typeDeclaration, addIdentifier.Identifier.ValueText, out var registration))
                {
                    if (registration.TryGetArgumentAtIndex(0, out var nameARg) &&
                        nameARg.TryGetStringValue(context.SemanticModel, context.CancellationToken, out var registeredName) &&
                        registeredName != eventSymbol.Name)
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.WPF0102EventDeclarationName,
                                eventDeclaration.Identifier.GetLocation(),
                                ImmutableDictionary<string, string?>.Empty.Add("ExpectedName", registeredName),
                                registeredName));
                    }

                    if (registration.TryGetArgumentAtIndex(2, out var handlerTypeArg) &&
                        handlerTypeArg.TryGetTypeofValue(context.SemanticModel, context.CancellationToken, out var registeredHandlerType) &&
                        !TypeSymbolComparer.Equal(registeredHandlerType, eventSymbol.Type))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.WPF0106EventDeclarationUseRegisteredHandlerType,
                                eventDeclaration.Type.GetLocation(),
                                registeredHandlerType.MetadataName));
                    }
                }
            }
        }
    }

    private class EventDeclarationWalker : PooledWalker<EventDeclarationWalker>
    {
        private InvocationExpressionSyntax addCall = null!;
        private InvocationExpressionSyntax removeCall = null!;

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            if (node.TryGetMethodName(out var name) &&
                name is "AddHandler" or "RemoveHandler" &&
                node.FirstAncestor<AccessorDeclarationSyntax>() is { } accessor)
            {
                if (accessor.IsKind(SyntaxKind.AddAccessorDeclaration))
                {
                    this.addCall = node;
                }
                else if (accessor.IsKind(SyntaxKind.RemoveAccessorDeclaration))
                {
                    this.removeCall = node;
                }
            }

            base.VisitInvocationExpression(node);
        }

        internal static bool TryGetCalls(EventDeclarationSyntax eventDeclaration, out InvocationExpressionSyntax addCall, out InvocationExpressionSyntax removeCall)
        {
            using var walker = BorrowAndVisit(eventDeclaration, () => new EventDeclarationWalker());
            addCall = walker.addCall;
            removeCall = walker.removeCall;
            return addCall is { } || removeCall is { };
        }

        protected override void Clear()
        {
            this.addCall = null!;
            this.removeCall = null!;
        }
    }

    private class BackingFieldWalker : PooledWalker<BackingFieldWalker>
    {
        private VariableDeclaratorSyntax backingField = null!;
        private PropertyDeclarationSyntax backingProperty = null!;
        private string memberName = null!;

        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            if (node.Declaration.Variables.TrySingle(out var variable) &&
                variable.Identifier.ValueText == this.memberName)
            {
                this.backingField = variable;
            }

            base.VisitFieldDeclaration(node);
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            if (node.Identifier.ValueText == this.memberName)
            {
                this.backingProperty = node;
            }

            base.VisitPropertyDeclaration(node);
        }

        internal static bool TryGetRegistration(TypeDeclarationSyntax typeDeclaration, string memberName, [NotNullWhen(true)] out InvocationExpressionSyntax? registration)
        {
            registration = null;
            if (typeDeclaration is null ||
                string.IsNullOrEmpty(memberName))
            {
                return false;
            }

            using var walker = Borrow(() => new BackingFieldWalker());
            walker.memberName = memberName;
            walker.Visit(typeDeclaration);
            if (walker.backingField is { Initializer.Value: InvocationExpressionSyntax fieldRegistration })
            {
                registration = fieldRegistration;
            }
            else if (walker.backingProperty is { Initializer.Value: InvocationExpressionSyntax propertyRegistration })
            {
                registration = propertyRegistration;
            }
            else
            {
                return false;
            }

            return registration is { ArgumentList.Arguments.Count: 4 } &&
                   registration.TryGetMethodName(out var name) &&
                   name == "RegisterRoutedEvent";
        }

        protected override void Clear()
        {
            this.backingField = null!;
            this.backingProperty = null!;
            this.memberName = null!;
        }
    }
}
