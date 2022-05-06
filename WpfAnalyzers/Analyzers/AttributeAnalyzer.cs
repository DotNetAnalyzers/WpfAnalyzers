namespace WpfAnalyzers;

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using Gu.Roslyn.AnalyzerExtensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class AttributeAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Descriptors.WPF0008DependsOnTarget,
        Descriptors.WPF0051XmlnsDefinitionMustMapExistingNamespace,
        Descriptors.WPF0081MarkupExtensionReturnTypeMustUseCorrectType,
        Descriptors.WPF0082ConstructorArgument,
        Descriptors.WPF0084XamlSetMarkupExtensionAttributeTarget,
        Descriptors.WPF0085XamlSetTypeConverterTarget,
        Descriptors.WPF0132UsePartPrefix,
        Descriptors.WPF0133ContentPropertyTarget,
        Descriptors.WPF0150UseNameofInsteadOfLiteral,
        Descriptors.WPF0151UseNameofInsteadOfConstant,
        Descriptors.WPF0170StyleTypedPropertyPropertyTarget,
        Descriptors.WPF0171StyleTypedPropertyPropertyType,
        Descriptors.WPF0172StyleTypedPropertyPropertySpecified,
        Descriptors.WPF0173StyleTypedPropertyStyleTargetType,
        Descriptors.WPF0174StyleTypedPropertyStyleSpecified,
        Descriptors.WPF0175StyleTypedPropertyPropertyUnique);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(x => Handle(x), SyntaxKind.Attribute);
    }

    private static void Handle(SyntaxNodeAnalysisContext context)
    {
        if (!context.IsExcludedFromAnalysis() &&
            context.ContainingSymbol is { } &&
            context.Node is AttributeSyntax attribute)
        {
            if (context.SemanticModel.TryGetNamedType(attribute, KnownSymbols.DependsOnAttribute, context.CancellationToken, out _) &&
                TryFindStringArgument(context, attribute, 0, "name", out var argument, out var expression, out var text))
            {
                if (TryFindPropertyRecursive(context.ContainingSymbol.ContainingType, text!, out var property))
                {
                    if (!argument.Expression.IsNameof())
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                expression.IsKind(SyntaxKind.StringLiteralExpression)
                                    ? Descriptors.WPF0150UseNameofInsteadOfLiteral
                                    : Descriptors.WPF0151UseNameofInsteadOfConstant,
                                expression.GetLocation(),
                                ImmutableDictionary<string, string?>.Empty.Add(nameof(IdentifierNameSyntax), property.Name),
                                property.Name));
                    }
                }
                else
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0008DependsOnTarget, expression.GetLocation()));
                }
            }
            else if (context.SemanticModel.TryGetNamedType(attribute, KnownSymbols.XmlnsDefinitionAttribute, context.CancellationToken, out _) &&
                     TryFindStringArgument(context, attribute, 1, KnownSymbols.XmlnsDefinitionAttribute.ClrNamespaceArgumentName, out _, out expression, out text) &&
                     !TryFindNamespaceRecursive(context, text, out _))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0051XmlnsDefinitionMustMapExistingNamespace, expression.GetLocation(), expression));
            }
            else if (context.SemanticModel.TryGetNamedType(attribute, KnownSymbols.MarkupExtensionReturnTypeAttribute, context.CancellationToken, out _) &&
                     context.ContainingSymbol is ITypeSymbol { IsAbstract: false } containingType &&
                     containingType.IsAssignableTo(KnownSymbols.MarkupExtension, context.SemanticModel.Compilation) &&
                     attribute.TryFirstAncestor<ClassDeclarationSyntax>(out var classDeclaration) &&
                     MarkupExtension.TryGetReturnType(classDeclaration, context.SemanticModel, context.CancellationToken, out var returnType) &&
                     returnType != KnownSymbols.Object &&
                     TryFindTypeArgument(context, attribute, 0, "returnType", out expression, out var argumentType) &&
                     !returnType.IsAssignableTo(argumentType, context.SemanticModel.Compilation))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptors.WPF0081MarkupExtensionReturnTypeMustUseCorrectType,
                        expression.GetLocation(),
                        properties: ImmutableDictionary<string, string?>.Empty.Add(nameof(ITypeSymbol), returnType.ToMinimalDisplayString(context.SemanticModel, context.Node.SpanStart)),
                        returnType));
            }
            else if (context.SemanticModel.TryGetNamedType(attribute, KnownSymbols.ConstructorArgumentAttribute, context.CancellationToken, out _) &&
                     ConstructorArgument.TryGetArgumentName(attribute, out argument, out var argumentName) &&
                     context.ContainingProperty() is { } containingProperty &&
                     ConstructorArgument.TryGetParameterName(containingProperty, context.SemanticModel, context.CancellationToken, out var parameterName) &&
                     argumentName != parameterName)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptors.WPF0082ConstructorArgument,
                        argument.GetLocation(),
                        ImmutableDictionary<string, string?>.Empty.Add(nameof(ConstructorArgument), parameterName),
                        parameterName));
            }
            else if (context.SemanticModel.TryGetNamedType(attribute, KnownSymbols.XamlSetMarkupExtensionAttribute, context.CancellationToken, out _) &&
                     TryFindStringArgument(context, attribute, 0, "xamlSetMarkupExtensionHandler", out argument, out expression, out text))
            {
                if (TryFindMethodRecursive(context.ContainingSymbol as INamedTypeSymbol, text, m => IsMarkupExtensionHandler(m), out var method))
                {
                    if (!argument.Expression.IsNameof())
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                expression.IsKind(SyntaxKind.StringLiteralExpression)
                                    ? Descriptors.WPF0150UseNameofInsteadOfLiteral
                                    : Descriptors.WPF0151UseNameofInsteadOfConstant,
                                expression.GetLocation(),
                                ImmutableDictionary<string, string?>.Empty.Add(nameof(IdentifierNameSyntax), method.Name),
                                method.Name));
                    }
                }
                else
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0084XamlSetMarkupExtensionAttributeTarget, expression.GetLocation()));
                }
            }
            else if (context.SemanticModel.TryGetNamedType(attribute, KnownSymbols.XamlSetTypeConverterAttribute, context.CancellationToken, out _) &&
                     TryFindStringArgument(context, attribute, 0, "xamlSetTypeConverterHandler", out argument, out expression, out text))
            {
                if (TryFindMethodRecursive(context.ContainingSymbol as INamedTypeSymbol, text, m => IsTypeConverterHandler(m), out var method))
                {
                    if (!argument.Expression.IsNameof())
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                expression.IsKind(SyntaxKind.StringLiteralExpression)
                                    ? Descriptors.WPF0150UseNameofInsteadOfLiteral
                                    : Descriptors.WPF0151UseNameofInsteadOfConstant,
                                expression.GetLocation(),
                                ImmutableDictionary<string, string?>.Empty.Add(nameof(IdentifierNameSyntax), method.Name),
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
                     context.SemanticModel.TryGetConstantValue(argument.Expression, context.CancellationToken, out string? partName) &&
                     partName?.StartsWith("PART_", StringComparison.Ordinal) != true)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0132UsePartPrefix, argument.Expression.GetLocation(), argument));
            }
            else if (context.SemanticModel.TryGetNamedType(attribute, KnownSymbols.ContentPropertyAttribute, context.CancellationToken, out _) &&
                     TryFindStringArgument(context, attribute, 0, "name", out argument, out expression, out text))
            {
                if (TryFindPropertyRecursive(context.ContainingSymbol as INamedTypeSymbol, text, out var property))
                {
                    if (!argument.Expression.IsNameof())
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                expression.IsKind(SyntaxKind.StringLiteralExpression)
                                    ? Descriptors.WPF0150UseNameofInsteadOfLiteral
                                    : Descriptors.WPF0151UseNameofInsteadOfConstant,
                                expression.GetLocation(),
                                ImmutableDictionary<string, string?>.Empty.Add(nameof(IdentifierNameSyntax), property.Name),
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
                if (TryFindStringArgument(context, attribute, 0, "Property", out argument, out expression, out text))
                {
                    if (TryFindStyleTypedProperty(context, text, out var property, out var styleType))
                    {
                        if (property is { } &&
                            !argument.Expression.IsNameof())
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    expression.IsKind(SyntaxKind.StringLiteralExpression)
                                        ? Descriptors.WPF0150UseNameofInsteadOfLiteral
                                        : Descriptors.WPF0151UseNameofInsteadOfConstant,
                                    expression.GetLocation(),
                                    ImmutableDictionary<string, string?>.Empty.Add(nameof(IdentifierNameSyntax), property.Name),
                                    property.Name));
                        }

                        if (!styleType.Is(KnownSymbols.Style))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0171StyleTypedPropertyPropertyType, expression.GetLocation()));
                        }

                        if (FindDuplicateStyleTypedProperty())
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0175StyleTypedPropertyPropertyUnique, expression.GetLocation()));
                        }

                        bool FindDuplicateStyleTypedProperty()
                        {
                            if (attribute.TryFirstAncestor(out ClassDeclarationSyntax? attributedType))
                            {
                                foreach (var list in attributedType.AttributeLists)
                                {
                                    foreach (var candidate in list.Attributes)
                                    {
                                        if (!ReferenceEquals(candidate, attribute) &&
                                            context.SemanticModel.TryGetNamedType(candidate, KnownSymbols.StyleTypedPropertyAttribute, context.CancellationToken, out _) &&
                                            TryFindStringArgument(context, candidate, 0, "Property", out _, out _, out var other) &&
                                            other == text)
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }

                            return false;
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

                if (TryFindTypeArgument(context, attribute, 1, "StyleTargetType", out expression, out argumentType))
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
    }

    private static bool TryFindStringArgument(SyntaxNodeAnalysisContext context, AttributeSyntax candidate, int index, string name, [NotNullWhen(true)] out AttributeArgumentSyntax? argument, out ExpressionSyntax expression, [NotNullWhen(true)] out string? text)
    {
        text = null;
        if (candidate.TryFindArgument(index, name, out argument))
        {
            return TryFindExpression(argument, out expression) &&
                   argument.TryGetStringValue(context.SemanticModel, context.CancellationToken, out text);
        }

        argument = null;
        expression = candidate.Name;
        return false;

        static bool TryFindExpression(AttributeArgumentSyntax a, out ExpressionSyntax result)
        {
            switch (a.Expression)
            {
                case LiteralExpressionSyntax literal:
                    result = literal;
                    return true;
                case InvocationExpressionSyntax invocation when invocation.IsNameOf() &&
                                                                invocation.TrySingleArgument(out var nameArg):
                    result = nameArg.Expression;
                    return true;
                default:
                    result = a.Expression;
                    return result is { };
            }
        }
    }

    private static bool TryFindTypeArgument(SyntaxNodeAnalysisContext context, AttributeSyntax candidate, int index, string name, [NotNullWhen(true)] out ExpressionSyntax? expression, [NotNullWhen(true)] out ITypeSymbol? type)
    {
        expression = null;
        type = null;
        return candidate.TryFindArgument(index, name, out var argument) &&
               TryFindExpression(out expression) &&
               context.SemanticModel.TryGetType(expression!, context.CancellationToken, out type);

        bool TryFindExpression(out ExpressionSyntax? result)
        {
            switch (argument!.Expression)
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

    private static bool TryFindNamespaceRecursive(SyntaxNodeAnalysisContext context, string name, [NotNullWhen(true)] out INamespaceSymbol? result)
    {
        foreach (var ns in context.SemanticModel.Compilation.GlobalNamespace.GetNamespaceMembers())
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

            foreach (var nested in symbol.GetNamespaceMembers())
            {
                if (TryFindRecursive(nested, out match))
                {
                    return true;
                }
            }

            match = null!;
            return false;
        }
    }

    private static bool TryFindPropertyRecursive(ITypeSymbol? type, string name, [NotNullWhen(true)] out IPropertySymbol? result)
    {
        result = null;
        return type is { } &&
               type.TryFindPropertyRecursive(name, out result);
    }

    private static bool TryFindStyleTypedProperty(SyntaxNodeAnalysisContext context, string name, out IPropertySymbol? result, [NotNullWhen(true)] out ITypeSymbol? registeredType)
    {
        if (context.ContainingSymbol is INamedTypeSymbol type)
        {
            if (type.TryFindPropertyRecursive(name, out result))
            {
                registeredType = result.Type;
                return true;
            }

            foreach (var member in type.GetMembers())
            {
                if (BackingFieldOrProperty.TryCreateCandidate(member, out var backing) &&
                    backing.RegisteredName(context.SemanticModel, context.CancellationToken) is { Value: { } registeredName } &&
                    registeredName == name &&
                    backing.RegisteredType(context.SemanticModel, context.CancellationToken) is { Value: { } match })
                {
                    registeredType = match;
                    return true;
                }
            }
        }

        result = null;
        registeredType = null;
        return false;
    }

    private static bool TryFindMethodRecursive(ITypeSymbol? type, string name, Func<IMethodSymbol, bool> selector, [NotNullWhen(true)] out IMethodSymbol? result)
    {
        result = null;
        return type is { } &&
               type.TryFindFirstMethodRecursive(name, selector, out result);
    }

    private static bool IsMarkupExtensionHandler(IMethodSymbol candidate)
    {
        return candidate is { ReturnsVoid: true, Parameters.Length: 2 } &&
               candidate.Parameters.TryElementAt<IParameterSymbol>(0, out var parameter) &&
               parameter.Type == KnownSymbols.Object &&
               candidate.Parameters.TryElementAt<IParameterSymbol>(1, out parameter) &&
               parameter.Type == KnownSymbols.XamlSetMarkupExtensionEventArgs;
    }

    private static bool IsTypeConverterHandler(IMethodSymbol candidate)
    {
        return candidate is { ReturnsVoid: true, Parameters.Length: 2 } &&
               candidate.Parameters.TryElementAt<IParameterSymbol>(0, out var parameter) &&
               parameter.Type == KnownSymbols.Object &&
               candidate.Parameters.TryElementAt<IParameterSymbol>(1, out parameter) &&
               parameter.Type == KnownSymbols.XamlSetTypeConverterEventArgs;
    }
}
