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
    internal class RoutedEventCallbackAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            WPF0090RegisterClassHandlerCallbackNameShouldMatchEvent.Descriptor,
            WPF0091AddAndRemoveHandlerCallbackNameShouldMatchEvent.Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(x => Handle(x), SyntaxKind.Argument);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is ArgumentSyntax argument &&
                argument.Expression is ObjectCreationExpressionSyntax objectCreation &&
                objectCreation.TrySingleArgument(out var callbackArg) &&
                callbackArg.Expression is IdentifierNameSyntax &&
                objectCreation.Parent is ArgumentSyntax handlerArgument &&
                handlerArgument.FirstAncestor<InvocationExpressionSyntax>() is InvocationExpressionSyntax invocation)
            {
                if (EventManager.TryGetRegisterClassHandlerCall(invocation, context.SemanticModel, context.CancellationToken, out _) &&
                    invocation.TryGetArgumentAtIndex(1, out var eventArgument))
                {
                    HandleCallback(context, eventArgument, callbackArg, WPF0090RegisterClassHandlerCallbackNameShouldMatchEvent.Descriptor);
                }
                else if ((TryGetAddHandlerCall(invocation, context, out _) ||
                          TryGetRemoveHandlerCall(invocation, context, out _)) &&
                          invocation.TryGetArgumentAtIndex(0, out eventArgument))
                {
                    HandleCallback(context, eventArgument, callbackArg, WPF0091AddAndRemoveHandlerCallbackNameShouldMatchEvent.Descriptor);
                }
            }
        }

        private static void HandleCallback(SyntaxNodeAnalysisContext context, ArgumentSyntax eventArgument, ArgumentSyntax callbackArg, DiagnosticDescriptor descriptor)
        {
            var invokedHandler = (IdentifierNameSyntax)callbackArg.Expression;
            if (eventArgument.Expression is IdentifierNameSyntax identifierName &&
                EventManager.IsMatch(invokedHandler.Identifier.ValueText, identifierName.Identifier.ValueText) == false)
            {
                if (EventManager.TryGetExpectedCallbackName(identifierName.Identifier.ValueText, out var expectedName))
                {
                    var properties = ImmutableDictionary.CreateRange(new[] { new KeyValuePair<string, string>("ExpectedName", expectedName) });
                    context.ReportDiagnostic(Diagnostic.Create(descriptor, callbackArg.GetLocation(), properties, expectedName));
                }
                else
                {
                    context.ReportDiagnostic(Diagnostic.Create(descriptor, callbackArg.GetLocation(), "On" + identifierName.Identifier.ValueText));
                }
            }

            if (eventArgument.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Name is IdentifierNameSyntax nameSyntax &&
                EventManager.IsMatch(invokedHandler.Identifier.ValueText, nameSyntax.Identifier.ValueText) == false)
            {
                if (EventManager.TryGetExpectedCallbackName(nameSyntax.Identifier.ValueText, out var expectedName))
                {
                    var properties = ImmutableDictionary.CreateRange(
                        new[] { new KeyValuePair<string, string>("ExpectedName", expectedName), });
                    context.ReportDiagnostic(
                        Diagnostic.Create(descriptor, callbackArg.GetLocation(), properties, expectedName));
                }
                else
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(descriptor, callbackArg.GetLocation(), "On" + nameSyntax.Identifier.ValueText));
                }
            }
        }

        private static bool TryGetAddHandlerCall(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ArgumentSyntax eventArgument)
        {
            eventArgument = null;
            if (invocation.TryGetMethodName(out var name) &&
                name != "AddHandler")
            {
                return false;
            }

            if (invocation.ArgumentList == null ||
                invocation.ArgumentList.Arguments.Count < 2 ||
                invocation.ArgumentList.Arguments.Count > 3)
            {
                return false;
            }

            return invocation.TryGetArgumentAtIndex(0, out eventArgument) &&
                   context.SemanticModel.GetTypeInfoSafe(eventArgument.Expression, context.CancellationToken).Type == KnownSymbol.RoutedEvent;
        }

        private static bool TryGetRemoveHandlerCall(InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context, out ArgumentSyntax eventArgument)
        {
            eventArgument = null;
            if (invocation.TryGetMethodName(out var name) &&
                name != "RemoveHandler")
            {
                return false;
            }

            if (invocation.ArgumentList == null ||
                invocation.ArgumentList.Arguments.Count != 2)
            {
                return false;
            }

            return invocation.TryGetArgumentAtIndex(0, out eventArgument) &&
                   context.SemanticModel.GetTypeInfoSafe(eventArgument.Expression, context.CancellationToken).Type == KnownSymbol.RoutedEvent;
        }
    }
}
