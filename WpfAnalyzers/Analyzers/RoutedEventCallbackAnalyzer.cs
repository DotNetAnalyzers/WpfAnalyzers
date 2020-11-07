namespace WpfAnalyzers
{
    using System.Collections.Immutable;

    using Gu.Roslyn.AnalyzerExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class RoutedEventCallbackAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Descriptors.WPF0090RegisterClassHandlerCallbackNameShouldMatchEvent,
            Descriptors.WPF0091AddAndRemoveHandlerCallbackNameShouldMatchEvent);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(x => Handle(x), SyntaxKind.Argument);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is ArgumentSyntax { Expression: ObjectCreationExpressionSyntax { Parent: ArgumentSyntax handlerArgument } objectCreation } &&
                objectCreation.TrySingleArgument(out var callbackArg) &&
                callbackArg.Expression is IdentifierNameSyntax && handlerArgument.FirstAncestor<InvocationExpressionSyntax>() is { } invocation)
            {
                if (EventManager.RegisterClassHandler.Match(invocation, context.SemanticModel, context.CancellationToken) is { EventArgument: { } eventArgument })
                {
                    HandleCallback(context, eventArgument, callbackArg, Descriptors.WPF0090RegisterClassHandlerCallbackNameShouldMatchEvent);
                }
                else if ((EventManager.AddHandler.Match(invocation, context.SemanticModel, context.CancellationToken) is { } ||
                          EventManager.RemoveHandler.Match(invocation, context.SemanticModel, context.CancellationToken) is { }) &&
                          invocation.TryGetArgumentAtIndex(0, out eventArgument))
                {
                    HandleCallback(context, eventArgument, callbackArg, Descriptors.WPF0091AddAndRemoveHandlerCallbackNameShouldMatchEvent);
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
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            descriptor,
                            callbackArg.GetLocation(),
                            ImmutableDictionary<string, string?>.Empty.Add("ExpectedName", expectedName),
                            expectedName));
                }
                else
                {
                    context.ReportDiagnostic(Diagnostic.Create(descriptor, callbackArg.GetLocation(), "On" + identifierName.Identifier.ValueText));
                }
            }

            if (eventArgument.Expression is MemberAccessExpressionSyntax { Name: IdentifierNameSyntax nameSyntax } &&
                EventManager.IsMatch(invokedHandler.Identifier.ValueText, nameSyntax.Identifier.ValueText) == false)
            {
                if (EventManager.TryGetExpectedCallbackName(nameSyntax.Identifier.ValueText, out var expectedName))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            descriptor,
                            callbackArg.GetLocation(),
                            ImmutableDictionary<string, string?>.Empty.Add("ExpectedName", expectedName),
                            expectedName));
                }
                else
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(descriptor, callbackArg.GetLocation(), "On" + nameSyntax.Identifier.ValueText));
                }
            }
        }
    }
}
