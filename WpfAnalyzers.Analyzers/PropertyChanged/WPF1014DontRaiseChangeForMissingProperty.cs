namespace WpfAnalyzers.PropertyChanged
{
    using System.Collections.Immutable;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    using WpfAnalyzers.PropertyChanged.Helpers;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF1014DontRaiseChangeForMissingProperty : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF1014";
        private const string Title = "Don't raise PropertyChanged for missing property.";
        private const string MessageFormat = "Don't raise PropertyChanged for missing property.";
        private const string Description = Title;
        private static readonly string HelpLink = WpfAnalyzers.HelpLink.ForId(DiagnosticId);

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            AnalyzerCategory.PropertyChanged,
            DiagnosticSeverity.Error,
            AnalyzerConstants.EnabledByDefault,
            Description,
            HelpLink);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(HandleInvocation, SyntaxKind.InvocationExpression);
        }

        private static void HandleInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            var method = context.SemanticModel.GetSymbolSafe(invocation, context.CancellationToken) as IMethodSymbol;
            if (method == KnownSymbol.PropertyChangedEventHandler.Invoke)
            {
                ArgumentSyntax eventArgs;
                if (invocation.ArgumentList.Arguments.TryGetAtIndex(1, out eventArgs))
                {
                    var objectCreation = eventArgs.Expression as ObjectCreationExpressionSyntax;
                    var symbol = context.SemanticModel.GetSymbolSafe(objectCreation, context.CancellationToken)?.ContainingType;
                    if (symbol != KnownSymbol.PropertyChangedEventArgs)
                    {
                        return;
                    }

                    ArgumentSyntax nameArg = null;
                    if (objectCreation?.ArgumentList.Arguments.TryGetSingle(out nameArg) == true)
                    {
                        if (IsForExistingProperty(context, nameArg))
                        {
                            return;
                        }

                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, nameArg.GetLocation()));
                    }
                }

                return;
            }

            if (invocation.ArgumentList?.Arguments.Count != 1)
            {
                return;
            }

            if (PropertyChanged.IsInvoker(method, context.SemanticModel, context.CancellationToken) != PropertyChanged.InvokesPropertyChanged.No)
            {
                var argument = invocation.ArgumentList.Arguments[0];
                if (IsForExistingProperty(context, argument))
                {
                    return;
                }

                context.ReportDiagnostic(Diagnostic.Create(Descriptor, argument.GetLocation()));
            }
        }

        private static bool IsForExistingProperty(SyntaxNodeAnalysisContext context, ArgumentSyntax argument)
        {
            string name;
            if (argument.TryGetStringValue(context.SemanticModel, context.CancellationToken, out name))
            {
                if (string.IsNullOrEmpty(name))
                {
                    return true;
                }

                if (name == "Item[]")
                {
                    foreach (var member in context.ContainingSymbol.ContainingType.RecursiveMembers())
                    {
                        var property = member as IPropertySymbol;
                        if (property?.IsIndexer == true)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                foreach (var member in context.ContainingSymbol.ContainingType.RecursiveMembers(name))
                {
                    var property = member as IPropertySymbol;
                    if (property?.IsIndexer == false)
                    {
                        return true;
                    }
                }

                return false;
            }

            return true;
        }
    }
}