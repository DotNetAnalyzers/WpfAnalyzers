namespace WpfAnalyzers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

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
            Descriptors.WPF0170StyleTypedPropertyPropertyTarget,
            Descriptors.WPF0171StyleTypedPropertyPropertyType,
            Descriptors.WPF0172StyleTypedPropertyPropertySpecified,
            Descriptors.WPF0173StyleTypedPropertyStyleTargetType,
            Descriptors.WPF0174StyleTypedPropertyStyleSpecified,
            Descriptors.WPF0175StyleTypedPropertyPropertyUnique);

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
                if (context.SemanticModel.TryGetNamedType(attribute, KnownSymbols.DependsOnAttribute, context.CancellationToken, out _) &&
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
                else if (context.SemanticModel.TryGetNamedType(attribute, KnownSymbols.XmlnsDefinitionAttribute, context.CancellationToken, out _) &&
                         TryFindStringArgument(attribute, 1, KnownSymbols.XmlnsDefinitionAttribute.ClrNamespaceArgumentName, out expression, out text) &&
                         !TryFindNamespaceRecursive(text, out _))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0051XmlnsDefinitionMustMapExistingNamespace, expression.GetLocation(), expression));
                }
                else if (context.SemanticModel.TryGetNamedType(attribute, KnownSymbols.MarkupExtensionReturnTypeAttribute, context.CancellationToken, out _) &&
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
                else if (context.SemanticModel.TryGetNamedType(attribute, KnownSymbols.ConstructorArgumentAttribute, context.CancellationToken, out _) &&
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
                else if (context.SemanticModel.TryGetNamedType(attribute, KnownSymbols.XamlSetMarkupExtensionAttribute, context.CancellationToken, out _) &&
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
                else if (context.SemanticModel.TryGetNamedType(attribute, KnownSymbols.XamlSetTypeConverterAttribute, context.CancellationToken, out _) &&
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
                else if (context.SemanticModel.TryGetNamedType(attribute, KnownSymbols.TemplatePartAttribute, context.CancellationToken, out _) &&
                         attribute.TryFindArgument(0, "Name", out argument) &&
                         context.SemanticModel.TryGetConstantValue(argument.Expression, context.CancellationToken, out string partName) &&
                         !partName.StartsWith("PART_"))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0132UsePartPrefix, argument.Expression.GetLocation(), argument));
                }
                else if (context.SemanticModel.TryGetNamedType(attribute, KnownSymbols.ContentPropertyAttribute, context.CancellationToken, out _) &&
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
                else if (context.SemanticModel.TryGetNamedType(attribute, KnownSymbols.StyleTypedPropertyAttribute, context.CancellationToken, out _))
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
                                context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0171StyleTypedPropertyPropertyType, expression.GetLocation()));
                            }

                            if (FindDuplicateStyleTypedProperty(property.Name))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0175StyleTypedPropertyPropertyUnique, expression.GetLocation()));
                            }
                        }
                        else
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0170StyleTypedPropertyPropertyTarget, expression.GetLocation()));
                        }
                    }
                    else
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0172StyleTypedPropertyPropertySpecified, expression.GetLocation()));
                    }

                    if (TryFindTypeArgument(attribute, 1, "StyleTargetType", out expression, out argumentType))
                    {
                        if (!argumentType.TryFindPropertyRecursive("Style", out _))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0173StyleTypedPropertyStyleTargetType, expression.GetLocation()));
                        }
                    }
                    else
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0174StyleTypedPropertyStyleSpecified, attribute.GetLocation()));
                    }
                }
            }

            bool TryFindStringArgument(AttributeSyntax candidate, int index, string name, out ExpressionSyntax expression, out string text)
            {
                expression = null;
                text = null;
                if (candidate.TryFindArgument(index, name, out var argument))
                {
                    return TryFindExpression(out expression) &&
                           context.SemanticModel.TryGetConstantValue(argument.Expression, context.CancellationToken, out text);
                }

                expression = candidate.Name;
                return false;

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
                return candidate.TryFindArgument(index, name, out var argument) &&
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

            bool TryFindNamespaceRecursive(string name, out INamespaceSymbol result)
            {
                foreach (var ns in context.Compilation.GlobalNamespace.GetNamespaceMembers())
                {
                    if (TryFindRecursive(ns, out result))
                    {
                        return true;
                    }
                }

                result = null;
                return false;

                bool TryFindRecursive(INamespaceSymbol symbol, out INamespaceSymbol match)
                {
                    if (NamespaceSymbolComparer.Equals(symbol, name))
                    {
                        match = symbol;
                        return true;
                    }

                    foreach (INamespaceSymbol nested in symbol.GetNamespaceMembers())
                    {
                        if (TryFindRecursive(nested, out match))
                        {
                            return true;
                        }
                    }

                    match = null;
                    return false;
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

            bool FindDuplicateStyleTypedProperty(string property)
            {
                if (attribute.TryFirstAncestor(out TypeDeclarationSyntax containingType))
                {
                    foreach (var list in containingType.AttributeLists)
                    {
                        foreach (var candidate in list.Attributes)
                        {
                            if (!ReferenceEquals(candidate, attribute) &&
                                context.SemanticModel.TryGetNamedType(attribute, KnownSymbols.StyleTypedPropertyAttribute, context.CancellationToken, out _) &&
                                TryFindStringArgument(attribute, 0, "Property", out _, out var text) &&
                                text == property)
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
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
