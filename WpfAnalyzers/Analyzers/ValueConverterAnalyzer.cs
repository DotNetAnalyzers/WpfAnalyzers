namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class ValueConverterAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Descriptors.WPF0070ConverterDoesNotHaveDefaultField,
            Descriptors.WPF0071ConverterDoesNotHaveAttribute,
            Descriptors.WPF0072ValueConversionMustUseCorrectTypes,
            Descriptors.WPF0073ConverterDoesNotHaveAttributeUnknownTypes,
            Descriptors.WPF0074DefaultMemberOfWrongType);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(x => Handle(x), SyntaxKind.ClassDeclaration);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.ContainingSymbol is INamedTypeSymbol { IsAbstract: false, IsStatic: false } type &&
                type.IsAssignableToEither(KnownSymbols.IValueConverter, KnownSymbols.IMultiValueConverter, context.SemanticModel.Compilation) &&
                context.Node is ClassDeclarationSyntax classDeclaration &&
                type.DeclaredAccessibility != Accessibility.Private &&
                type.DeclaredAccessibility != Accessibility.Protected)
            {
                if (!type.IsAssignableTo(KnownSymbols.MarkupExtension, context.SemanticModel.Compilation))
                {
                    if (ValueConverter.TryGetDefaultFieldsOrProperties(type, context.SemanticModel.Compilation, out var defaults))
                    {
                        foreach (var fieldOrProperty in defaults)
                        {
                            if (fieldOrProperty.TryGetAssignedValue(context.CancellationToken, out var assignedValue) &&
                                context.SemanticModel.TryGetType(assignedValue, context.CancellationToken, out var assignedType) &&
                                !TypeSymbolComparer.Equal(assignedType, type))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0074DefaultMemberOfWrongType, assignedValue.GetLocation()));
                            }
                        }
                    }
                    else if (!Virtual.HasVirtualOrAbstractOrProtectedMembers(type) &&
                             !type.Constructors.TryFirst(x => x.Parameters.Length > 0, out _) &&
                             !Mutable.HasMutableInstanceMembers(type) &&
                             !classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0070ConverterDoesNotHaveDefaultField, classDeclaration.Identifier.GetLocation()));
                    }
                }

                if (type.IsAssignableTo(KnownSymbols.IValueConverter, context.SemanticModel.Compilation))
                {
                    if (Attribute.TryFind(classDeclaration, KnownSymbols.ValueConversionAttribute, context.SemanticModel, context.CancellationToken, out var attribute))
                    {
                        if (ValueConverter.TryGetConversionTypes(classDeclaration, context.SemanticModel, context.CancellationToken, out var sourceType, out var targetType))
                        {
                            if (sourceType is { } &&
                                sourceType != QualifiedType.System.Object &&
                                attribute.TryFindArgument(0, "sourceType", out var arg) &&
                                arg.Expression is TypeOfExpressionSyntax sourceTypeOf &&
                                TypeSymbol.TryGet(sourceTypeOf, type, context.SemanticModel, context.CancellationToken, out var argType) &&
                                !TypeSymbolComparer.Equal(argType, sourceType))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0072ValueConversionMustUseCorrectTypes, arg.GetLocation(), sourceType));
                            }

                            if (attribute.TryFindArgument(1, "targetType", out arg) &&
                                arg.Expression is TypeOfExpressionSyntax targetTypeOf &&
                                TypeSymbol.TryGet(targetTypeOf, type, context.SemanticModel, context.CancellationToken, out argType) &&
                                !TypeSymbolComparer.Equal(argType, targetType))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0072ValueConversionMustUseCorrectTypes, arg.GetLocation(), targetType));
                            }
                        }
                    }
                    else if (!classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                ValueConverter.TryGetConversionTypes(classDeclaration, context.SemanticModel, context.CancellationToken, out _, out _)
                                    ? Descriptors.WPF0071ConverterDoesNotHaveAttribute
                                    : Descriptors.WPF0073ConverterDoesNotHaveAttributeUnknownTypes,
                                classDeclaration.Identifier.GetLocation()));
                    }
                }
            }
        }
    }
}
