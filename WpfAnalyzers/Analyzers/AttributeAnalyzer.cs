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
            Descriptors.WPF0150UseNameof);

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
                    attribute.TrySingleArgument(out var argument))
                {
                    if (TryFindPropertyRecursive(context.ContainingSymbol.ContainingType, argument, out var property))
                    {
                        if (argument.Expression.IsKind(SyntaxKind.StringLiteralExpression))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    Descriptors.WPF0150UseNameof,
                                    argument.GetLocation(),
                                    ImmutableDictionary<string, string>.Empty.Add(nameof(IdentifierNameSyntax), property.Name),
                                    property.Name));
                        }
                    }
                    else
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0008DependsOnTarget, argument.GetLocation()));
                    }
                }
                else if (Attribute.IsType(attribute, KnownSymbols.XmlnsDefinitionAttribute, context.SemanticModel, context.CancellationToken) &&
                         Attribute.TryFindArgument(attribute, 1, KnownSymbols.XmlnsDefinitionAttribute.ClrNamespaceArgumentName, out argument) &&
                         context.SemanticModel.TryGetConstantValue(argument.Expression, context.CancellationToken, out string @namespace) &&
                         context.Compilation.GetSymbolsWithName(x => !string.IsNullOrEmpty(x) && @namespace.EndsWith(x), SymbolFilter.Namespace)
                           .All(x => x.ToMinimalDisplayString(context.SemanticModel, 0) != @namespace))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0051XmlnsDefinitionMustMapExistingNamespace, argument.GetLocation(), argument));
                }
                else if (Attribute.IsType(attribute, KnownSymbols.MarkupExtensionReturnTypeAttribute, context.SemanticModel, context.CancellationToken) &&
                         context.ContainingSymbol is ITypeSymbol containingType &&
                         !containingType.IsAbstract &&
                         containingType.IsAssignableTo(KnownSymbols.MarkupExtension, context.Compilation) &&
                         attribute.TryFirstAncestor<ClassDeclarationSyntax>(out var classDeclaration) &&
                         MarkupExtension.TryGetReturnType(classDeclaration, context.SemanticModel, context.CancellationToken, out var returnType) &&
                         returnType != KnownSymbols.Object &&
                         Attribute.TryFindArgument(attribute, 0, "returnType", out argument) &&
                         argument.Expression is TypeOfExpressionSyntax typeOf &&
                         context.SemanticModel.TryGetType(typeOf.Type, context.CancellationToken, out var argType) &&
                         !returnType.IsAssignableTo(argType, context.Compilation))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0081MarkupExtensionReturnTypeMustUseCorrectType, argument.GetLocation(), returnType));
                }
                else if (Attribute.IsType(attribute, KnownSymbols.ConstructorArgumentAttribute, context.SemanticModel, context.CancellationToken) &&
                         ConstructorArgument.TryGetArgumentName(attribute, out argument, out var argumentName) &&
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
                         Attribute.TryFindArgument(attribute, 0, "xamlSetMarkupExtensionHandler", out argument))
                {
                    if (TryFindMethodRecursive(context.ContainingSymbol as INamedTypeSymbol, argument, m => IsMarkupExtensionHandler(m), out var method))
                    {
                        if (argument.Expression.IsKind(SyntaxKind.StringLiteralExpression))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    Descriptors.WPF0150UseNameof,
                                    argument.GetLocation(),
                                    ImmutableDictionary<string, string>.Empty.Add(nameof(IdentifierNameSyntax), method.Name),
                                    method.Name));
                        }
                    }
                    else
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0084XamlSetMarkupExtensionAttributeTarget, argument.Expression.GetLocation()));
                    }
                }
                else if (Attribute.IsType(attribute, KnownSymbols.XamlSetTypeConverterAttribute, context.SemanticModel, context.CancellationToken) &&
                         Attribute.TryFindArgument(attribute, 0, "xamlSetTypeConverterHandler", out argument))
                {
                    if (TryFindMethodRecursive(context.ContainingSymbol as INamedTypeSymbol, argument, m => IsTypeConverterHandler(m), out var method))
                    {
                        if (argument.Expression.IsKind(SyntaxKind.StringLiteralExpression))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    Descriptors.WPF0150UseNameof,
                                    argument.GetLocation(),
                                    ImmutableDictionary<string, string>.Empty.Add(nameof(IdentifierNameSyntax), method.Name),
                                    method.Name));
                        }
                    }
                    else
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0085XamlSetTypeConverterTarget, argument.Expression.GetLocation()));
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
                         attribute.TrySingleArgument(out argument))
                {
                    if (TryFindPropertyRecursive(context.ContainingSymbol as INamedTypeSymbol, argument, out var property))
                    {
                        if (argument.Expression.IsKind(SyntaxKind.StringLiteralExpression))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    Descriptors.WPF0150UseNameof,
                                    argument.GetLocation(),
                                    ImmutableDictionary<string, string>.Empty.Add(nameof(IdentifierNameSyntax), property.Name),
                                    property.Name));
                        }
                    }
                    else
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0133ContentPropertyTarget, argument.GetLocation()));
                    }
                }
            }

            bool TryFindPropertyRecursive(ITypeSymbol type, AttributeArgumentSyntax argument, out IPropertySymbol result)
            {
                result = null;
                return type != null &&
                       context.SemanticModel.TryGetConstantValue(argument.Expression, context.CancellationToken, out string name) &&
                       type.TryFindPropertyRecursive(name, out result);
            }

            bool TryFindMethodRecursive(ITypeSymbol type, AttributeArgumentSyntax argument, Func<IMethodSymbol, bool> selector, out IMethodSymbol result)
            {
                result = null;
                return type != null &&
                       context.SemanticModel.TryGetConstantValue(argument.Expression, context.CancellationToken, out string name) &&
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
