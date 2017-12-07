namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class RoutedEventEventDeclarationAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            WPF0102EventDeclarationName.Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Handle, SyntaxKind.EventDeclaration);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.ContainingSymbol is IEventSymbol eventSymbol &&
                context.Node is EventDeclarationSyntax eventDeclaration &&
                EventDeclaration.TryGetAddAndRemoveHandler(eventDeclaration, out var addHandler, out var removeHandler))
            {
                if (addHandler.TryGetArgumentAtIndex(0, out var addArg) &&
                    addArg.Expression is IdentifierNameSyntax addIdentifier &&
                    removeHandler.TryGetArgumentAtIndex(0, out var removeArg) &&
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
                    else if (BackingField.TryGetRegistration(eventDeclaration.Parent as TypeDeclarationSyntax, addIdentifier.Identifier.ValueText, out var registration) &&
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
                                    ImmutableDictionary<string, string>.Empty.Add("RegisteredName", registeredName),
                                    registeredName));
                        }
                    }
                }
            }
        }

        private class EventDeclaration : PooledWalker<EventDeclaration>
        {
            private InvocationExpressionSyntax addHandler;
            private InvocationExpressionSyntax removeHandler;

            public static bool TryGetAddAndRemoveHandler(EventDeclarationSyntax eventDeclaration, out InvocationExpressionSyntax addHandler, out InvocationExpressionSyntax removeHandler)
            {
                using (var walker = BorrowAndVisit(eventDeclaration, () => new EventDeclaration()))
                {
                    addHandler = walker.addHandler;
                    removeHandler = walker.removeHandler;
                    return addHandler != null || removeHandler != null;
                }
            }

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                if (node.TryGetInvokedMethodName(out var name))
                {
                    if (name == "AddHandler")
                    {
                        this.addHandler = node;
                    }
                    else if (name == "RemoveHandler")
                    {
                        this.removeHandler = node;
                    }
                }

                base.VisitInvocationExpression(node);
            }

            protected override void Clear()
            {
                this.addHandler = null;
                this.removeHandler = null;
            }
        }

        private class BackingField : PooledWalker<BackingField>
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

                using (var walker = Borrow(() => new BackingField()))
                {
                    walker.memberName = memberName;
                    walker.Visit(typeDeclaration);
                    if (walker.backingField != null)
                    {
                        registration = walker.backingField.Initializer.Value as InvocationExpressionSyntax;
                    }
                    else if (walker.backingProperty != null)
                    {
                        registration = walker.backingProperty.Initializer.Value as InvocationExpressionSyntax;
                    }
                    else
                    {
                        return false;
                    }

                    return registration?.ArgumentList != null &&
                           registration.ArgumentList.Arguments.Count == 4 &&
                           registration.TryGetInvokedMethodName(out var name) &&
                           name == "RegisterRoutedEvent";
                }
            }

            public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
            {
                if (node.Declaration.Variables.TryGetSingle(out var variable) &&
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