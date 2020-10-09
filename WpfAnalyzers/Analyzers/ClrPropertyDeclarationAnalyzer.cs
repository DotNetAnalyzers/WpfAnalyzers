namespace WpfAnalyzers
{
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
                context.ContainingSymbol is IPropertySymbol { IsStatic: false } property &&
                property.ContainingType.IsAssignableTo(KnownSymbols.DependencyObject, context.Compilation) &&
                context.Node is PropertyDeclarationSyntax propertyDeclaration &&
                PropertyDeclarationWalker.TryGetCalls(propertyDeclaration, out var getCall, out var setCall))
            {
                if (getCall is { } &&
                    propertyDeclaration.TryGetGetter(out var getter) &&
                    getter.Body is { Statements: { } getStatements } &&
                    getStatements.TryFirst(x => !x.Contains(getCall), out var statement))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0036AvoidSideEffectsInClrAccessors, statement.GetLocation()));
                }

                if (setCall is { })
                {
                    if (propertyDeclaration.TryGetSetter(out var setter) &&
                        setter.Body is { Statements: { } setStatements } &&
                        setStatements.TryFirst(x => !x.Contains(setCall), out var sideEffect))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0036AvoidSideEffectsInClrAccessors, sideEffect.GetLocation()));
                    }

                    if (setCall.TryGetMethodName(out var setCallName) &&
                        setCallName != "SetValue")
                    {
                        //// ReSharper disable once PossibleNullReferenceException
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.WPF0035ClrPropertyUseSetValueInSetter,
                                setCall.GetLocation(),
                                context.ContainingSymbol.Name));
                    }
                }

                if (IsGettingAndSettingDifferent() == false)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptors.WPF0032ClrPropertyGetAndSetSameDependencyProperty,
                            propertyDeclaration.GetLocation(),
                            context.ContainingSymbol.Name));
                }

                if (ClrProperty.TryGetRegisterField(propertyDeclaration, context.SemanticModel, context.CancellationToken, out var fieldOrProperty))
                {
                    if (DependencyProperty.TryGetRegisteredName(fieldOrProperty, context.SemanticModel, context.CancellationToken, out _, out var registeredName) &&
                        registeredName != property.Name)
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.WPF0003ClrPropertyShouldMatchRegisteredName,
                                propertyDeclaration.Identifier.GetLocation(),
                                ImmutableDictionary<string, string>.Empty.Add("ExpectedName", registeredName),
                                property.Name,
                                registeredName));
                    }

                    if (DependencyProperty.TryGetRegisteredType(fieldOrProperty, context.SemanticModel, context.CancellationToken, out var registeredType))
                    {
                        if (!registeredType.Equals(property.Type))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    Descriptors.WPF0012ClrPropertyShouldMatchRegisteredType,
                                    propertyDeclaration.Type.GetLocation(),
                                    ImmutableDictionary<string, string>.Empty.Add(nameof(TypeSyntax), registeredType.ToMinimalDisplayString(context.SemanticModel, context.Node.SpanStart)),
                                    property,
                                    registeredType));
                        }
                        else if (getCall is { Parent: CastExpressionSyntax { Type: { } type } } &&
                                 context.SemanticModel.TryGetType(type, context.CancellationToken, out var castType) &&
                                 !registeredType.Equals(castType))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    Descriptors.WPF0012ClrPropertyShouldMatchRegisteredType,
                                    type.GetLocation(),
                                    ImmutableDictionary<string, string>.Empty.Add(nameof(TypeSyntax), registeredType.ToMinimalDisplayString(context.SemanticModel, context.Node.SpanStart)),
                                    property,
                                    registeredType));
                        }
                    }
                }

                bool? IsGettingAndSettingDifferent()
                {
                    if (getCall.TryGetArgumentAtIndex(0, out var getArg) &&
                        getArg.Expression is IdentifierNameSyntax getIdentifier &&
                        setCall.TryGetArgumentAtIndex(0, out var setArg) &&
                        setArg.Expression is IdentifierNameSyntax setIdentifier)
                    {
                        if (getIdentifier.Identifier.ValueText == setIdentifier.Identifier.ValueText)
                        {
                            return true;
                        }

                        if (context.SemanticModel.TryGetSymbol(getIdentifier, context.CancellationToken, out var getSymbol) &&
                            BackingFieldOrProperty.TryCreateCandidate(getSymbol, out var getBacking) &&
                            context.SemanticModel.TryGetSymbol(setIdentifier, context.CancellationToken, out var setSymbol) &&
                            BackingFieldOrProperty.TryCreateCandidate(setSymbol, out var setBacking) &&
                            DependencyProperty.TryGetRegisterInvocationRecursive(getBacking, context.SemanticModel, context.CancellationToken, out var getRegistration, out _) &&
                            DependencyProperty.TryGetRegisterInvocationRecursive(setBacking, context.SemanticModel, context.CancellationToken, out var setRegistration, out _))
                        {
                            return getRegistration == setRegistration;
                        }
                    }

                    return null;
                }
            }
        }

        private class PropertyDeclarationWalker : PooledWalker<PropertyDeclarationWalker>
        {
            private InvocationExpressionSyntax? getCall;
            private InvocationExpressionSyntax? setCall;

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                if (node.TryGetMethodName(out var name) &&
                    node.FirstAncestor<AccessorDeclarationSyntax>() is { } accessor)
                {
                    if (accessor.IsKind(SyntaxKind.SetAccessorDeclaration) &&
                        (name == "SetValue" || name == "SetCurrentValue"))
                    {
                        this.setCall = node;
                    }
                    else if (accessor.IsKind(SyntaxKind.GetAccessorDeclaration) &&
                             name == "GetValue")
                    {
                        this.getCall = node;
                    }
                }

                base.VisitInvocationExpression(node);
            }

            internal static bool TryGetCalls(PropertyDeclarationSyntax declaration, out InvocationExpressionSyntax? getCall, out InvocationExpressionSyntax? setCall)
            {
                using var walker = BorrowAndVisit(declaration, () => new PropertyDeclarationWalker());
                getCall = walker.getCall;
                setCall = walker.setCall;
                return getCall != null || setCall != null;
            }

            protected override void Clear()
            {
                this.getCall = null;
                this.setCall = null;
            }
        }
    }
}
