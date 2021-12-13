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
                callbackArg.Expression is IdentifierNameSyntax &&
                handlerArgument.FirstAncestor<InvocationExpressionSyntax>() is { } invocation)
            {
                if (EventManager.RegisterClassHandler.Match(invocation, context.SemanticModel, context.CancellationToken) is { Target: { } target, EventArgument: { } eventArgument })
                {
                    if (Callback.SingleInvocation(target, invocation.FirstAncestorOrSelf<TypeDeclarationSyntax>(), context) is { } &&
                        CheckName(context, eventArgument, callbackArg) is var (messageArg, properties))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.WPF0090RegisterClassHandlerCallbackNameShouldMatchEvent,
                                callbackArg.GetLocation(),
                                properties,
                                messageArg));
                    }
                }
                else if ((EventManager.AddHandler.Match(invocation, context.SemanticModel, context.CancellationToken) is { } ||
                          EventManager.RemoveHandler.Match(invocation, context.SemanticModel, context.CancellationToken) is { }) &&
                          invocation.TryGetArgumentAtIndex(0, out eventArgument))
                {
                    if (CheckName(context, eventArgument, callbackArg) is var (messageArg, properties))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.WPF0091AddAndRemoveHandlerCallbackNameShouldMatchEvent,
                                callbackArg.GetLocation(),
                                properties,
                                messageArg));
                    }
                }
            }
        }

        private static (string MessageArg, ImmutableDictionary<string, string?> Properties)? CheckName(SyntaxNodeAnalysisContext context, ArgumentSyntax eventArgument, ArgumentSyntax callbackArg)
        {
            if (callbackArg.Expression is IdentifierNameSyntax invokedHandler &&
                Identifier() is { } identifier)
            {
                if (EventManager.IsMatch(invokedHandler.Identifier.ValueText, identifier.Identifier.ValueText) == false)
                {
                    if (EventManager.TryGetExpectedCallbackName(identifier.Identifier.ValueText, out var expectedName))
                    {
                        return (expectedName, ImmutableDictionary<string, string?>.Empty.Add("ExpectedName", expectedName));
                    }
                    else
                    {
                        return ("On" + identifier.Identifier.ValueText, ImmutableDictionary<string, string?>.Empty);
                    }
                }
            }

            return null;

            IdentifierNameSyntax? Identifier()
            {
                return eventArgument switch
                {
                    { Expression: IdentifierNameSyntax name } => name,
                    { Expression: MemberAccessExpressionSyntax { Name: IdentifierNameSyntax name } } => name,
                    _ => null,
                };
            }
        }
    }
}
