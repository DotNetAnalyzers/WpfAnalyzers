namespace WpfAnalyzers;

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
            context.Node is ArgumentSyntax { Expression: ObjectCreationExpressionSyntax { } } argument &&
            argument.FirstAncestor<InvocationExpressionSyntax>() is { } invocation)
        {
            if (EventManager.RegisterClassHandler.Match(invocation, context.SemanticModel, context.CancellationToken) is { } registerClassHandler)
            {
                if (ShouldRename(registerClassHandler.Target, registerClassHandler.EventArgument, registerClassHandler.DelegateArgument) is var (location, properties, expectedName))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptors.WPF0090RegisterClassHandlerCallbackNameShouldMatchEvent,
                            location,
                            properties,
                            expectedName));
                }
            }
            else if (EventManager.AddHandler.Match(invocation, context.SemanticModel, context.CancellationToken) is { } addHandler)
            {
                if (ShouldRename(addHandler.Target, addHandler.EventArgument, addHandler.DelegateArgument) is var (location, properties, expectedName))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptors.WPF0091AddAndRemoveHandlerCallbackNameShouldMatchEvent,
                            location,
                            properties,
                            expectedName));
                }
            }
            else if (EventManager.RemoveHandler.Match(invocation, context.SemanticModel, context.CancellationToken) is { } removeHandler)
            {
                if (ShouldRename(removeHandler.Target, removeHandler.EventArgument, removeHandler.DelegateArgument) is var (location, properties, expectedName))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptors.WPF0091AddAndRemoveHandlerCallbackNameShouldMatchEvent,
                            location,
                            properties,
                            expectedName));
                }
            }

            (Location Location, ImmutableDictionary<string, string?> Properties, string ExpectedName)? ShouldRename(IMethodSymbol target, ArgumentSyntax eventArgument, ArgumentSyntax callbackArg)
            {
                if (CallbackIdentifier() is { } handler &&
                    Identifier(eventArgument.Expression) is { } eventField)
                {
                    if (EventManager.IsMatch(handler.Identifier.ValueText, eventField.Identifier.ValueText) == false &&
                        Callback.SingleInvocation(target, invocation.FirstAncestorOrSelf<TypeDeclarationSyntax>(), context) is { })
                    {
                        if (EventManager.TryGetExpectedCallbackName(eventField.Identifier.ValueText, out var expectedName))
                        {
                            return (handler.GetLocation(), ImmutableDictionary<string, string?>.Empty.Add("ExpectedName", expectedName), expectedName);
                        }

                        return (handler.GetLocation(), ImmutableDictionary<string, string?>.Empty, "On" + eventField.Identifier.ValueText);
                    }
                }

                return null;

                IdentifierNameSyntax? CallbackIdentifier()
                {
                    return callbackArg switch
                    {
                        { Expression: ObjectCreationExpressionSyntax { ArgumentList.Arguments: { Count: 1 } arguments } }
                            => Identifier(arguments[0].Expression),
                        _ => Identifier(callbackArg.Expression),
                    };
                }

                IdentifierNameSyntax? Identifier(ExpressionSyntax expression)
                {
                    return expression switch
                    {
                        IdentifierNameSyntax name => name,
                        MemberAccessExpressionSyntax { Name: IdentifierNameSyntax name } => name,
                        _ => null,
                    };
                }
            }
        }
    }
}
