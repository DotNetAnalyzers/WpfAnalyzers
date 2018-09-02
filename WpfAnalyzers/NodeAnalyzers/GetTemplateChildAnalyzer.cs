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
    internal class GetTemplateChildAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            WPF0130UseTemplatePartAttribute.Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(x => Handle(x), SyntaxKind.InvocationExpression);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.Node is InvocationExpressionSyntax invocation &&
                invocation.TryGetMethodName(out var name) &&
                name == "GetTemplateChild" &&
                invocation.ArgumentList is ArgumentListSyntax argumentList &&
                argumentList.Arguments.TrySingle(out var argument) &&
                context.SemanticModel.TryGetConstantValue(argument.Expression, context.CancellationToken, out string partName) &&
                context.ContainingSymbol is IMethodSymbol containingMethod &&
                containingMethod.Name == "OnApplyTemplate" &&
                containingMethod.IsOverride &&
                containingMethod.Parameters.Length == 0)
            {
                if (TryFindAttribute(containingMethod.ContainingType, partName, out var attribute))
                {
                }
                else
                {
                    var partNameArg = argument.Expression is LiteralExpressionSyntax
                        ? $"\"{partName}\""
                        : argument.Expression.ToString();

                    if (TryGetCastType(invocation, out var partType))
                    {
                        var attributeText = $"[System.Windows.TemplatePartAttribute(Name = {partNameArg}, Type = typeof({partType}))]";
                        context.ReportDiagnostic(Diagnostic.Create(
                                                     WPF0130UseTemplatePartAttribute.Descriptor,
                                                     invocation.GetLocation(),
                                                     ImmutableDictionary<string, string>.Empty.Add(nameof(AttributeListSyntax), attributeText),
                                                     attributeText));
                    }
                    else
                    {
                        var attributeText = $"[System.Windows.TemplatePartAttribute(Name = {partNameArg})]";
                        context.ReportDiagnostic(Diagnostic.Create(
                                                     WPF0130UseTemplatePartAttribute.Descriptor,
                                                     invocation.GetLocation(),
                                                     ImmutableDictionary<string, string>.Empty.Add(nameof(AttributeListSyntax), attributeText),
                                                     attributeText));
                    }
                }
            }
        }

        private static bool TryFindAttribute(INamedTypeSymbol type, string part, out AttributeData attribute)
        {
            attribute = null;
            if (type == null ||
                type == KnownSymbol.Object)
            {
                return false;
            }

            foreach (var candidate in type.GetAttributes())
            {
                if (candidate.AttributeClass == KnownSymbol.TemplatePartAttribute &&
                    candidate.NamedArguments.TryFirst(IsMatch, out _))
                {
                    attribute = candidate;
                    return true;
                }
            }

            return TryFindAttribute(type.BaseType, part, out attribute);

            bool IsMatch(KeyValuePair<string, TypedConstant> a)
            {
                return a.Key == "Name" &&
                       a.Value.Value is string candidate &&
                       candidate == part;
            }
        }

        private static bool TryGetCastType(InvocationExpressionSyntax invocation, out TypeSyntax type)
        {
            switch (invocation.Parent)
            {
                case CastExpressionSyntax castExpression:
                    type = castExpression.Type;
                    return true;
                case IsPatternExpressionSyntax isPattern when isPattern.Pattern is DeclarationPatternSyntax declarationPattern:
                    type = declarationPattern.Type;
                    return !type.IsVar;
                default:
                    type = null;
                    return false;
            }
        }
    }
}
