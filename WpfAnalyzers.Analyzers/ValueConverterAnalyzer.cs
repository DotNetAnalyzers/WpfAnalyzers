namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class ValueConverterAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            WPF0070ConverterDoesNotHaveDefaultField.Descriptor,
            WPF0071ConverterDoesNotHaveAttribute.Descriptor,
            WPF0072ValueConversionMustUseCorrectTypes.Descriptor,
            WPF0073ConverterDoesNotHaveAttributeUnknownTypes.Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(HandleDeclaration, SyntaxKind.ClassDeclaration);
        }

        private static void HandleDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.ContainingSymbol is ITypeSymbol type &&
                type.IsEither(KnownSymbol.IValueConverter, KnownSymbol.IMultiValueConverter) &&
                context.Node is ClassDeclarationSyntax classDeclaration &&
                !type.IsAbstract &&
                type.DeclaredAccessibility != Accessibility.Private &&
                type.DeclaredAccessibility != Accessibility.Protected)
            {
                if (!type.Is(KnownSymbol.MarkupExtension) &&
                    !Mutable.HasMutableInstanceMembers(type) &&
                    !Virtual.HasVirtualOrAbstractOrProtectedMembers(type) &&
                    !ValueConverter.TryGetDefaultFieldsOrProperties(type, out _))
                {
                    context.ReportDiagnostic(Diagnostic.Create(WPF0070ConverterDoesNotHaveDefaultField.Descriptor, classDeclaration.Identifier.GetLocation()));
                }

                if (type.Is(KnownSymbol.IValueConverter))
                {
                    if (Attribute.TryGetAttribute(classDeclaration, KnownSymbol.ValueConversionAttribute, context.SemanticModel, context.CancellationToken, out var attribute))
                    {
                        if (ValueConverter.TryGetConversionTypes(classDeclaration, context.SemanticModel, context.CancellationToken, out var sourceType, out var targetType))
                        {
                            if (Attribute.TryGetArgument(attribute, 0, "sourceType", out var arg) &&
                                arg.Expression is TypeOfExpressionSyntax typeOf &&
                                TypeOf.TryGetType(typeOf, context.SemanticModel, context.CancellationToken, out var argType) &&
                                !Equals(argType, sourceType))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(WPF0072ValueConversionMustUseCorrectTypes.Descriptor, arg.GetLocation(), argType));
                            }

                            if (Attribute.TryGetArgument(attribute, 1, "targetType", out arg) &&
                                arg.Expression is TypeOfExpressionSyntax typeOfExpression &&
                                TypeOf.TryGetType(typeOfExpression, context.SemanticModel, context.CancellationToken, out argType) &&
                                !Equals(argType, targetType))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(WPF0072ValueConversionMustUseCorrectTypes.Descriptor, arg.GetLocation(), argType));
                            }
                        }
                    }
                    else
                    {
                        if (ValueConverter.TryGetConversionTypes(classDeclaration, context.SemanticModel, context.CancellationToken, out _, out _))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(WPF0071ConverterDoesNotHaveAttribute.Descriptor, classDeclaration.Identifier.GetLocation()));
                        }
                        else
                        {
                            context.ReportDiagnostic(Diagnostic.Create(WPF0073ConverterDoesNotHaveAttributeUnknownTypes.Descriptor, classDeclaration.Identifier.GetLocation()));
                        }
                    }
                }
            }
        }
    }
}