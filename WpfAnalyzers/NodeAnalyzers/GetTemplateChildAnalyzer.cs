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
            WPF0130UseTemplatePartAttribute.Descriptor,
            WPF0131TemplatePartType.Descriptor);

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
                    if (TryGetCastType(invocation, out var cast, out var castTypeSyntax))
                    {
                        if (TryFindTemplatePartType(attribute, out var partType))
                        {
                            if (partType != null &&
                                context.SemanticModel.TryGetType(castTypeSyntax, context.CancellationToken, out var castType) &&
                                !IsValidCast(partType, castType, cast, context.Compilation))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(WPF0131TemplatePartType.Descriptor, invocation.GetLocation()));
                            }
                        }
                        else
                        {
                            context.ReportDiagnostic(Diagnostic.Create(WPF0131TemplatePartType.Descriptor, invocation.GetLocation()));
                        }
                    }
                }
                else
                {
                    var partNameArg = argument.Expression is LiteralExpressionSyntax
                        ? $"\"{partName}\""
                        : argument.Expression.ToString();

                    if (TryGetCastType(invocation, out _, out var partType))
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
                    candidate.NamedArguments.TryFirst(x => IsMatch(x), out _))
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

        private static bool TryFindTemplatePartType(AttributeData attribute, out INamedTypeSymbol type)
        {
            type = null;
            if (attribute.NamedArguments.TrySingle(x => x.Key == "Type", out var arg))
            {
                type = arg.Value.Value as INamedTypeSymbol;
            }

            return type != null;
        }

        private static bool TryGetCastType(InvocationExpressionSyntax invocation, out ExpressionSyntax cast, out SyntaxNode type)
        {
            switch (invocation.Parent)
            {
                case BinaryExpressionSyntax binary when
                    binary.IsKind(SyntaxKind.AsExpression):
                    {
                        cast = binary;
                        type = binary.Right as TypeSyntax;
                        return true;
                    }

                case CastExpressionSyntax castExpression:
                    cast = castExpression;
                    type = castExpression.Type;
                    return true;
                case IsPatternExpressionSyntax isPattern when
                    isPattern.Pattern is DeclarationPatternSyntax declarationPattern &&
                    !declarationPattern.Type.IsVar:
                    {
                        cast = isPattern;
                        type = declarationPattern.Type;
                        return true;
                    }

                default:
                    type = null;
                    cast = null;
                    return false;
            }
        }

        private static bool IsValidCast(INamedTypeSymbol partType, ITypeSymbol castType, ExpressionSyntax cast, Compilation compilation)
        {
            if (partType.IsAssignableTo(castType, compilation))
            {
                return true;
            }

            if (!(cast is CastExpressionSyntax))
            {
                return castType.IsAssignableTo(partType, compilation);
            }

            return false;
        }
    }
}
