namespace WpfAnalyzers
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
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
            WPF0051XmlnsDefinitionMustMapExistingNamespace.Descriptor,
            WPF0081MarkupExtensionReturnTypeMustUseCorrectType.Descriptor,
            WPF0082ConstructorArgument.Descriptor,
            WPF0132UsePartPrefix.Descriptor);

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
                if (Attribute.IsType(attribute, KnownSymbol.XmlnsDefinitionAttribute, context.SemanticModel, context.CancellationToken) &&
                    Attribute.TryFindArgument(attribute, 1, KnownSymbol.XmlnsDefinitionAttribute.ClrNamespaceArgumentName, out var arg) &&
                    context.SemanticModel.TryGetConstantValue(arg.Expression, context.CancellationToken, out string @namespace) &&
                    context.Compilation.GetSymbolsWithName(x => !string.IsNullOrEmpty(x) && @namespace.EndsWith(x), SymbolFilter.Namespace)
                           .All(x => x.ToMinimalDisplayString(context.SemanticModel, 0) != @namespace))
                {
                    context.ReportDiagnostic(Diagnostic.Create(WPF0051XmlnsDefinitionMustMapExistingNamespace.Descriptor, arg.GetLocation(), arg));
                }
                else if (context.ContainingSymbol is ITypeSymbol type &&
                         !type.IsAbstract &&
                         type.IsAssignableTo(KnownSymbol.MarkupExtension, context.Compilation) &&
                         Attribute.IsType(attribute, KnownSymbol.MarkupExtensionReturnTypeAttribute, context.SemanticModel, context.CancellationToken) &&
                         attribute.TryFirstAncestor<ClassDeclarationSyntax>(out var classDeclaration) &&
                         MarkupExtension.TryGetReturnType(classDeclaration, context.SemanticModel, context.CancellationToken, out var returnType) &&
                         returnType != KnownSymbol.Object &&
                         Attribute.TryFindArgument(attribute, 0, "returnType", out arg) &&
                         arg.Expression is TypeOfExpressionSyntax typeOf &&
                         context.SemanticModel.TryGetType(typeOf.Type, context.CancellationToken, out var argType) &&
                         !ReferenceEquals(argType, returnType))
                {
                    context.ReportDiagnostic(Diagnostic.Create(WPF0081MarkupExtensionReturnTypeMustUseCorrectType.Descriptor, arg.GetLocation(), returnType));
                }
                else if (Attribute.IsType(attribute, KnownSymbol.ConstructorArgumentAttribute, context.SemanticModel, context.CancellationToken) &&
                         ConstructorArgument.TryGetArgumentName(attribute, out var argument, out var argumentName) &&
                         attribute.TryFirstAncestor<PropertyDeclarationSyntax>(out var propertyDeclaration) &&
                         context.SemanticModel.TryGetSymbol(propertyDeclaration, context.CancellationToken, out var property) &&
                         ConstructorArgument.TryGetParameterName(property, context.SemanticModel, context.CancellationToken, out var parameterName) &&
                         argumentName != parameterName)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            WPF0082ConstructorArgument.Descriptor,
                            argument.GetLocation(),
                            ImmutableDictionary.CreateRange(new[] { new KeyValuePair<string, string>(nameof(ConstructorArgument), parameterName) }),
                            parameterName));
                }
                else if (Attribute.IsType(attribute, KnownSymbol.TemplatePartAttribute, context.SemanticModel, context.CancellationToken) &&
                         Attribute.TryFindArgument(attribute, 0, "Name", out arg) &&
                         context.SemanticModel.TryGetConstantValue(arg.Expression, context.CancellationToken, out string partName) &&
                         !partName.StartsWith("PART_"))
                {
                    context.ReportDiagnostic(Diagnostic.Create(WPF0132UsePartPrefix.Descriptor, arg.Expression.GetLocation(), arg));
                }
            }
        }
    }
}
