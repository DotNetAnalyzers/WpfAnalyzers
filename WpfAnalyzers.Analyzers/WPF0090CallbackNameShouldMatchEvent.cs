namespace WpfAnalyzers
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0090CallbackNameShouldMatchEvent : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0090";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Name the invoked method OnEventName.",
            messageFormat: "Rename to {0} to match the event.",
            category: AnalyzerCategory.RoutedEvent,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Name the invoked method OnEventName.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(HandleDeclaration, SyntaxKind.Argument);
        }

        private static void HandleDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.Node is ArgumentSyntax argument &&
                argument.Expression is ObjectCreationExpressionSyntax objectCreation &&
                objectCreation.ArgumentList.Arguments.TryGetSingle(out var callbackArg) &&
                callbackArg.Expression is IdentifierNameSyntax &&
                objectCreation.Parent is ArgumentSyntax handlerArgument &&
                handlerArgument.FirstAncestor<InvocationExpressionSyntax>() is InvocationExpressionSyntax invocation)
            {
                if (EventManager.TryRegisterClassHandlerCall(invocation, context.SemanticModel, context.CancellationToken, out _) &&
                    invocation.TryGetArgumentAtIndex(1, out var eventArgument))
                {
                    HandleCallback(context, eventArgument, callbackArg);
                }
            }
        }

        private static void HandleCallback(SyntaxNodeAnalysisContext context, ArgumentSyntax eventArgument, ArgumentSyntax callbackArg)
        {
            var invokedHandler = (IdentifierNameSyntax)callbackArg.Expression;
            if (eventArgument.Expression is IdentifierNameSyntax identifierName &&
                EventManager.IsMatch(invokedHandler.Identifier.ValueText, identifierName.Identifier.ValueText) == false)
            {
                if (EventManager.TryGetExpectedCallbackName(identifierName.Identifier.ValueText, out var expectedName))
                {
                    var properties = ImmutableDictionary.CreateRange(new[] { new KeyValuePair<string, string>("ExpectedName", expectedName) });
                    context.ReportDiagnostic(
                        Diagnostic.Create(Descriptor, callbackArg.GetLocation(), properties, expectedName));
                }
                else
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(Descriptor, callbackArg.GetLocation(), "On" + identifierName.Identifier.ValueText));
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
                        Diagnostic.Create(Descriptor, callbackArg.GetLocation(), properties, expectedName));
                }
                else
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(Descriptor, callbackArg.GetLocation(), "On" + nameSyntax.Identifier.ValueText));
                }
            }
        }
    }
}