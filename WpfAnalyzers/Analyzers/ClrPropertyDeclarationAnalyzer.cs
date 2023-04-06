namespace WpfAnalyzers;

using System.Collections.Immutable;

using Gu.Roslyn.AnalyzerExtensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class ClrPropertyDeclarationAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Descriptors.WPF0003ClrPropertyShouldMatchRegisteredName,
        Descriptors.WPF0012ClrPropertyShouldMatchRegisteredType,
        Descriptors.WPF0032ClrPropertyGetAndSetSameDependencyProperty,
        Descriptors.WPF0035ClrPropertyUseSetValueInSetter,
        Descriptors.WPF0036AvoidSideEffectsInClrAccessors);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(x => Handle(x), SyntaxKind.PropertyDeclaration);
    }

    private static void Handle(SyntaxNodeAnalysisContext context)
    {
        if (!context.IsExcludedFromAnalysis() &&
            context is { ContainingSymbol: IPropertySymbol { IsStatic: false } property, Node: PropertyDeclarationSyntax propertyDeclaration } &&
            propertyDeclaration.Getter() is { } getter &&
            propertyDeclaration.Setter() is { } setter)
        {
            if (ClrProperty.Match(property, context.SemanticModel, context.CancellationToken) is { SetValue: { } setValue, BackingSet: { } backingSet, GetValue: { } getValue, BackingGet: { } backingGet })
            {
                if (getter.Body is { Statements: { Count: > 1 } getStatements } &&
                    getStatements.TryFirst(x => !x.Contains(getValue), out var statement))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0036AvoidSideEffectsInClrAccessors, statement.GetLocation()));
                }

                if (setter.Body is { Statements: { Count: > 1 } setStatements } &&
                    setStatements.TryFirst(x => !x.Contains(setValue), out var sideEffect))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0036AvoidSideEffectsInClrAccessors, sideEffect.GetLocation()));
                }

                if (IsGettingAndSettingDifferent())
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptors.WPF0032ClrPropertyGetAndSetSameDependencyProperty,
                            propertyDeclaration.GetLocation(),
                            context.ContainingSymbol.Name));
                }

                if (backingGet.RegisteredName(context.SemanticModel, context.CancellationToken) is { Value: { } registeredName } &&
                    registeredName != property.Name)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptors.WPF0003ClrPropertyShouldMatchRegisteredName,
                            propertyDeclaration.Identifier.GetLocation(),
                            ImmutableDictionary<string, string?>.Empty.Add("ExpectedName", registeredName),
                            property.Name,
                            registeredName));
                }

                if (backingGet.RegisteredType(context.SemanticModel, context.CancellationToken) is { Value: { } registeredType })
                {
                    if (!MatchesRegisteredType(property.Type, registeredType))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.WPF0012ClrPropertyShouldMatchRegisteredType,
                                propertyDeclaration.Type.GetLocation(),
                                ImmutableDictionary<string, string?>.Empty.Add(nameof(TypeSyntax), registeredType.ToMinimalDisplayString(context.SemanticModel, context.Node.SpanStart)),
                                property,
                                registeredType));
                    }
                    else if (getValue is { Parent: CastExpressionSyntax { Type: { } type } } &&
                             context.SemanticModel.GetType(type, context.CancellationToken) is { } castType &&
                             !MatchesRegisteredType(castType, registeredType))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.WPF0012ClrPropertyShouldMatchRegisteredType,
                                type.GetLocation(),
                                ImmutableDictionary<string, string?>.Empty.Add(nameof(TypeSyntax), registeredType.ToMinimalDisplayString(context.SemanticModel, context.Node.SpanStart)),
                                property,
                                registeredType));
                    }

                    static bool MatchesRegisteredType(ITypeSymbol candidate, ITypeSymbol registeredType)
                    {
                        if (TypeSymbolComparer.Equal(candidate, registeredType))
                        {
                            return true;
                        }

                        if (candidate.NullableAnnotation == NullableAnnotation.Annotated &&
                            registeredType.NullableAnnotation == NullableAnnotation.NotAnnotated)
                        {
                            return TypeSymbolComparer.Equal(candidate.OriginalDefinition, registeredType);
                        }

                        return false;
                    }
                }

                bool IsGettingAndSettingDifferent()
                {
                    if (DependencyProperty.Register.FindRecursive(backingGet, context.SemanticModel, context.CancellationToken) is { Invocation: { } getRegistration } &&
                        DependencyProperty.Register.FindRecursive(backingSet, context.SemanticModel, context.CancellationToken) is { Invocation: { } setRegistration })
                    {
                        return getRegistration != setRegistration;
                    }

                    return false;
                }
            }

            if (DependencyObject.SetCurrentValue.Find(MethodOrAccessor.Create(setter), context.SemanticModel, context.CancellationToken) is { Invocation: { } setCurrentValue })
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptors.WPF0035ClrPropertyUseSetValueInSetter,
                        setCurrentValue.GetLocation(),
                        context.ContainingSymbol.Name));
            }
        }
    }
}
