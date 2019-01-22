namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class RoutedEventEventDeclarationAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            WPF0102EventDeclarationName.Descriptor,
            WPF0103EventDeclarationAddRemove.Descriptor,
            WPF0104EventDeclarationAddHandlerInAdd.Descriptor,
            WPF0105EventDeclarationRemoveHandlerInRemove.Descriptor,
            WPF0106EventDeclarationUseRegisteredHandlerType.Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(x => Handle(x), SyntaxKind.EventDeclaration);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.ContainingSymbol is IEventSymbol eventSymbol &&
                context.Node is EventDeclarationSyntax eventDeclaration &&
                EventDeclarationWalker.TryGetCalls(eventDeclaration, out var addCall, out var removeCall))
            {
                if (addCall.TryGetMethodName(out var addName) &&
                    addName != "AddHandler")
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            WPF0104EventDeclarationAddHandlerInAdd.Descriptor,
                            addCall.GetLocation()));
                }

                if (removeCall.TryGetMethodName(out var removeName) &&
                    removeName != "RemoveHandler")
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            WPF0105EventDeclarationRemoveHandlerInRemove.Descriptor,
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
                                WPF0103EventDeclarationAddRemove.Descriptor,
                                eventDeclaration.Identifier.GetLocation(),
                                addIdentifier.Identifier.ValueText,
                                removeIdentifier.Identifier.ValueText));
                    }
                    else if (BackingFieldWalker.TryGetRegistration(eventDeclaration.Parent as TypeDeclarationSyntax, addIdentifier.Identifier.ValueText, out var registration) &&
                             registration.ArgumentList != null)
                    {
                        if (registration.TryGetArgumentAtIndex(0, out var nameARg) &&
                            nameARg.TryGetStringValue(context.SemanticModel, context.CancellationToken, out var registeredName) &&
                            registeredName != eventSymbol.Name)
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    WPF0102EventDeclarationName.Descriptor,
                                    eventDeclaration.Identifier.GetLocation(),
                                    ImmutableDictionary<string, string>.Empty.Add("ExpectedName", registeredName),
                                    registeredName));
                        }

                        if (registration.TryGetArgumentAtIndex(2, out var handlerTypeArg) &&
                            handlerTypeArg.TryGetTypeofValue(context.SemanticModel, context.CancellationToken, out var registeredHandlerType) &&
                            !registeredHandlerType.Equals(eventSymbol.Type))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    WPF0106EventDeclarationUseRegisteredHandlerType.Descriptor,
                                    eventDeclaration.Type.GetLocation(),
                                    registeredHandlerType.MetadataName));
                        }
                    }
                }
            }
        }

        private class EventDeclarationWalker : PooledWalker<EventDeclarationWalker>
        {
            private InvocationExpressionSyntax addCall;
            private InvocationExpressionSyntax removeCall;

            public static bool TryGetCalls(EventDeclarationSyntax eventDeclaration, out InvocationExpressionSyntax addCall, out InvocationExpressionSyntax removeCall)
            {
                using (var walker = BorrowAndVisit(eventDeclaration, () => new EventDeclarationWalker()))
                {
                    addCall = walker.addCall;
                    removeCall = walker.removeCall;
                    return addCall != null || removeCall != null;
                }
            }

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                if (node.TryGetMethodName(out var name) &&
                    (name == "AddHandler" || name == "RemoveHandler") &&
                    node.FirstAncestor<AccessorDeclarationSyntax>() is AccessorDeclarationSyntax accessor)
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

            protected override void Clear()
            {
                this.addCall = null;
                this.removeCall = null;
            }
        }

        private class BackingFieldWalker : PooledWalker<BackingFieldWalker>
        {
            private VariableDeclaratorSyntax backingField;
            private PropertyDeclarationSyntax backingProperty;
            private string memberName;

            public static bool TryGetRegistration(TypeDeclarationSyntax typeDeclaration, string memberName, out InvocationExpressionSyntax registration)
            {
                registration = null;
                if (typeDeclaration == null ||
                    string.IsNullOrEmpty(memberName))
                {
                    return false;
                }

                using (var walker = Borrow(() => new BackingFieldWalker()))
                {
                    walker.memberName = memberName;
                    walker.Visit(typeDeclaration);
                    if (walker.backingField is VariableDeclaratorSyntax variableDeclarator &&
                        variableDeclarator.Initializer is EqualsValueClauseSyntax fieldInitializer &&
                        fieldInitializer.Value is InvocationExpressionSyntax fieldRegistration)
                    {
                        registration = fieldRegistration;
                    }
                    else if (walker.backingProperty is PropertyDeclarationSyntax propertyDeclaration &&
                             propertyDeclaration.Initializer is EqualsValueClauseSyntax propertyInitializer &&
                             propertyInitializer.Value is InvocationExpressionSyntax propertyRegistration)
                    {
                        registration = propertyRegistration;
                    }
                    else
                    {
                        return false;
                    }

                    return registration?.ArgumentList != null &&
                           registration.ArgumentList.Arguments.Count == 4 &&
                           registration.TryGetMethodName(out var name) &&
                           name == "RegisterRoutedEvent";
                }
            }

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

            protected override void Clear()
            {
                this.backingField = null;
                this.backingProperty = null;
                this.memberName = null;
            }
        }
    }
}
