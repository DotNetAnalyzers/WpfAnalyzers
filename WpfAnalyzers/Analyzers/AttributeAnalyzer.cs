namespace WpfAnalyzers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Attribute = Gu.Roslyn.AnalyzerExtensions.Attribute;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class AttributeAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Descriptors.WPF0008DependsOnTarget,
            Descriptors.WPF0051XmlnsDefinitionMustMapExistingNamespace,
            Descriptors.WPF0081MarkupExtensionReturnTypeMustUseCorrectType,
            Descriptors.WPF0082ConstructorArgument,
            Descriptors.WPF0084XamlSetMarkupExtensionAttributeTarget,
            Descriptors.WPF0085XamlSetTypeConverterTarget,
            Descriptors.WPF0132UsePartPrefix,
            Descriptors.WPF0133ContentPropertyTarget,
            Descriptors.WPF0150UseNameof,
            Descriptors.WPF0170StyleTypedPropertyTarget,
            Descriptors.WPF0171StyleTypedPropertyType,
            Descriptors.WPF0172StyleTypedPropertyProvided,
            Descriptors.WPF0173StyleTypedPropertyStyleTargetType);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(x => Handle(x), SyntaxKind.Attribute);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is AttributeSyntax attribute)
            {
                if (Attribute.IsType(attribute, KnownSymbols.DependsOnAttribute, context.SemanticModel, context.CancellationToken) &&
                    TryFindStringArgument(attribute, 0, "name", out var expression, out string text))
                {
                    if (TryFindPropertyRecursive(context.ContainingSymbol.ContainingType, text, out var property))
                    {
                        if (expression.IsKind(SyntaxKind.StringLiteralExpression))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    Descriptors.WPF0150UseNameof,
                                    expression.GetLocation(),
                                    ImmutableDictionary<string, string>.Empty.Add(nameof(IdentifierNameSyntax), property.Name),
                                    property.Name));
                        }
                    }
                    else
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0008DependsOnTarget, expression.GetLocation()));
                    }
                }
                else if (Attribute.IsType(attribute, KnownSymbols.XmlnsDefinitionAttribute, context.SemanticModel, context.CancellationToken) &&
                         TryFindStringArgument(attribute, 1, KnownSymbols.XmlnsDefinitionAttribute.ClrNamespaceArgumentName, out expression, out text) &&
                         context.Compilation.GetSymbolsWithName(x => !string.IsNullOrEmpty(x) && text.EndsWith(x), SymbolFilter.Namespace)
                           .All(x => x.ToMinimalDisplayString(context.SemanticModel, 0) != text))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0051XmlnsDefinitionMustMapExistingNamespace, expression.GetLocation(), expression));
                }
                else if (Attribute.IsType(attribute, KnownSymbols.MarkupExtensionReturnTypeAttribute, context.SemanticModel, context.CancellationToken) &&
                         context.ContainingSymbol is ITypeSymbol containingType &&
                         !containingType.IsAbstract &&
                         containingType.IsAssignableTo(KnownSymbols.MarkupExtension, context.Compilation) &&
                         attribute.TryFirstAncestor<ClassDeclarationSyntax>(out var classDeclaration) &&
                         MarkupExtension.TryGetReturnType(classDeclaration, context.SemanticModel, context.CancellationToken, out var returnType) &&
                         returnType != KnownSymbols.Object &&
                         TryFindTypeArgument(attribute, 0, "returnType", out expression, out var argumentType) &&
                         !returnType.IsAssignableTo(argumentType, context.Compilation))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptors.WPF0081MarkupExtensionReturnTypeMustUseCorrectType,
                            expression.GetLocation(),
                            properties: ImmutableDictionary<string, string>.Empty.Add(nameof(ITypeSymbol), returnType.ToMinimalDisplayString(context.SemanticModel, context.Node.SpanStart)),
                            returnType));
                }
                else if (Attribute.IsType(attribute, KnownSymbols.ConstructorArgumentAttribute, context.SemanticModel, context.CancellationToken) &&
                         ConstructorArgument.TryGetArgumentName(attribute, out var argument, out var argumentName) &&
                         ConstructorArgument.TryGetParameterName(context.ContainingProperty(), context.SemanticModel, context.CancellationToken, out var parameterName) &&
                         argumentName != parameterName)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptors.WPF0082ConstructorArgument,
                            argument.GetLocation(),
                            ImmutableDictionary.CreateRange(new[] { new KeyValuePair<string, string>(nameof(ConstructorArgument), parameterName) }),
                            parameterName));
                }
                else if (Attribute.IsType(attribute, KnownSymbols.XamlSetMarkupExtensionAttribute, context.SemanticModel, context.CancellationToken) &&
                         TryFindStringArgument(attribute, 0, "xamlSetMarkupExtensionHandler", out expression, out text))
                {
                    if (TryFindMethodRecursive(context.ContainingSymbol as INamedTypeSymbol, text, m => IsMarkupExtensionHandler(m), out var method))
                    {
                        if (expression.IsKind(SyntaxKind.StringLiteralExpression))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    Descriptors.WPF0150UseNameof,
                                    expression.GetLocation(),
                                    ImmutableDictionary<string, string>.Empty.Add(nameof(IdentifierNameSyntax), method.Name),
                                    method.Name));
                        }
                    }
                    else
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0084XamlSetMarkupExtensionAttributeTarget, expression.GetLocation()));
                    }
                }
                else if (Attribute.IsType(attribute, KnownSymbols.XamlSetTypeConverterAttribute, context.SemanticModel, context.CancellationToken) &&
                         TryFindStringArgument(attribute, 0, "xamlSetTypeConverterHandler", out expression, out text))
                {
                    if (TryFindMethodRecursive(context.ContainingSymbol as INamedTypeSymbol, text, m => IsTypeConverterHandler(m), out var method))
                    {
                        if (expression.IsKind(SyntaxKind.StringLiteralExpression))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    Descriptors.WPF0150UseNameof,
                                    expression.GetLocation(),
                                    ImmutableDictionary<string, string>.Empty.Add(nameof(IdentifierNameSyntax), method.Name),
                                    method.Name));
                        }
                    }
                    else
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0085XamlSetTypeConverterTarget, expression.GetLocation()));
                    }
                }
                else if (Attribute.IsType(attribute, KnownSymbols.TemplatePartAttribute, context.SemanticModel, context.CancellationToken) &&
                         Attribute.TryFindArgument(attribute, 0, "Name", out argument) &&
                         context.SemanticModel.TryGetConstantValue(argument.Expression, context.CancellationToken, out string partName) &&
                         !partName.StartsWith("PART_"))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0132UsePartPrefix, argument.Expression.GetLocation(), argument));
                }
                else if (Attribute.IsType(attribute, KnownSymbols.ContentPropertyAttribute, context.SemanticModel, context.CancellationToken) &&
                         TryFindStringArgument(attribute, 0, "name", out expression, out text))
                {
                    if (TryFindPropertyRecursive(context.ContainingSymbol as INamedTypeSymbol, text, out var property))
                    {
                        if (expression.IsKind(SyntaxKind.StringLiteralExpression))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    Descriptors.WPF0150UseNameof,
                                    expression.GetLocation(),
                                    ImmutableDictionary<string, string>.Empty.Add(nameof(IdentifierNameSyntax), property.Name),
                                    property.Name));
                        }
                    }
                    else
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0133ContentPropertyTarget, expression.GetLocation()));
                    }
                }
                else if (Attribute.IsType(attribute, KnownSymbols.StyleTypedPropertyAttribute, context.SemanticModel, context.CancellationToken))
                {
                    if (TryFindStringArgument(attribute, 0, "Property", out expression, out text))
                    {
                        if (TryFindPropertyRecursive(context.ContainingSymbol as INamedTypeSymbol, text, out var property))
                        {
                            if (expression.IsKind(SyntaxKind.StringLiteralExpression))
                            {
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        Descriptors.WPF0150UseNameof,
                                        expression.GetLocation(),
                                        ImmutableDictionary<string, string>.Empty.Add(nameof(IdentifierNameSyntax), property.Name),
                                        property.Name));
                            }

                            if (!property.Type.Is(KnownSymbols.Style))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0171StyleTypedPropertyType, expression.GetLocation()));
                            }
                        }
                        else
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0170StyleTypedPropertyTarget, expression.GetLocation()));
                        }
                    }
                    else
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0172StyleTypedPropertyProvided, expression.GetLocation()));
                    }

                    if (TryFindTypeArgument(attribute, 1, "StyleTargetType", out expression, out argumentType) &&
                        !argumentType.TryFindPropertyRecursive("Style", out _))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0173StyleTypedPropertyStyleTargetType, expression.GetLocation()));
                    }
                }
            }

            bool TryFindStringArgument(AttributeSyntax candidate, int index, string name, out ExpressionSyntax expression, out string text)
            {
                expression = null;
                text = null;
                if (Attribute.TryFindArgument(candidate, index, name, out var argument) &&
                    NameEquals())
                {
                    return TryFindExpression(out expression) &&
                           context.SemanticModel.TryGetConstantValue(argument.Expression, context.CancellationToken, out text);
                }

                expression = candidate.Name;
                return false;

                bool NameEquals()
                {
                    if (argument.NameEquals is NameEqualsSyntax nameEquals)
                    {
                        return nameEquals.Name.Identifier.ValueText == name;
                    }

                    return true;
                }

                bool TryFindExpression(out ExpressionSyntax result)
                {
                    switch (argument.Expression)
                    {
                        case LiteralExpressionSyntax literal:
                            result = literal;
                            return true;
                        case InvocationExpressionSyntax invocation when invocation.IsNameOf() &&
                                                                        invocation.TrySingleArgument(out var nameArg):
                            result = nameArg.Expression;
                            return true;
                        default:
                            result = null;
                            return false;
                    }
                }
            }

            bool TryFindTypeArgument(AttributeSyntax candidate, int index, string name, out ExpressionSyntax expression, out ITypeSymbol type)
            {
                expression = null;
                type = null;
                return Attribute.TryFindArgument(candidate, index, name, out var argument) &&
                       TryFindExpression(out expression) &&
                       context.SemanticModel.TryGetType(expression, context.CancellationToken, out type);

                bool TryFindExpression(out ExpressionSyntax result)
                {
                    switch (argument.Expression)
                    {
                        case TypeOfExpressionSyntax typeOf:
                            result = typeOf.Type;
                            return true;
                        default:
                            result = null;
                            return false;
                    }
                }
            }

            bool TryFindPropertyRecursive(ITypeSymbol type, string name, out IPropertySymbol result)
            {
                result = null;
                return type != null &&
                       type.TryFindPropertyRecursive(name, out result);
            }

            bool TryFindMethodRecursive(ITypeSymbol type, string name, Func<IMethodSymbol, bool> selector, out IMethodSymbol result)
            {
                result = null;
                return type != null &&
                       type.TryFindFirstMethodRecursive(name, selector, out result);
            }
        }

        private static bool IsMarkupExtensionHandler(IMethodSymbol candidate)
        {
            return candidate.ReturnsVoid &&
                   candidate.Parameters.Length == 2 &&
                   candidate.Parameters.TryElementAt(0, out var parameter) &&
                   parameter.Type == KnownSymbols.Object &&
                   candidate.Parameters.TryElementAt(1, out parameter) &&
                   parameter.Type == KnownSymbols.XamlSetMarkupExtensionEventArgs;
        }

        private static bool IsTypeConverterHandler(IMethodSymbol candidate)
        {
            return candidate.ReturnsVoid &&
                   candidate.Parameters.Length == 2 &&
                   candidate.Parameters.TryElementAt(0, out var parameter) &&
                   parameter.Type == KnownSymbols.Object &&
                   candidate.Parameters.TryElementAt(1, out parameter) &&
                   parameter.Type == KnownSymbols.XamlSetTypeConverterEventArgs;
        }
    }
}
