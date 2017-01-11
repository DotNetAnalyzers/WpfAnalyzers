namespace WpfAnalyzers.PropertyChanged
{
    using System.Collections.Immutable;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    using WpfAnalyzers.PropertyChanged.Helpers;

    using AnalysisResult = WpfAnalyzers.PropertyChanged.Helpers.AnalysisResult;

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
            DiagnosticSeverity.Warning,
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
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            var invocation = (InvocationExpressionSyntax)context.Node;
            var method = context.SemanticModel.GetSymbolSafe(invocation, context.CancellationToken) as IMethodSymbol;

            if (method == KnownSymbol.PropertyChangedEventHandler.Invoke ||
                PropertyChanged.IsInvoker(method, context.SemanticModel, context.CancellationToken) != AnalysisResult.No)
            {
                ArgumentSyntax nameArg;
                string propretyName;
                if (PropertyChanged.TryGetInvokedPropertyChangedName(invocation, context.SemanticModel, context.CancellationToken, out nameArg, out propretyName) == AnalysisResult.Yes)
                {
                    if (IsForExistingProperty(context, propretyName))
                    {
                        return;
                    }

                    if (invocation.Span.Contains(nameArg.Span))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, nameArg.GetLocation()));
                        return;
                    }

                    if (method == KnownSymbol.PropertyChangedEventHandler.Invoke)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocation.ArgumentList.Arguments[1].GetLocation()));
                        return;
                    }

                    if (invocation.ArgumentList.Arguments.Count == 1)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocation.ArgumentList.Arguments[0].GetLocation()));
                        return;
                    }

                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocation.GetLocation()));
                }
            }
        }

        private static bool IsForExistingProperty(SyntaxNodeAnalysisContext context, string name)
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
    }
}