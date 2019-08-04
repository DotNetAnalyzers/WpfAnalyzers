namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class OverrideMetadataAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Descriptors.WPF0017MetadataMustBeAssignable,
            Descriptors.WPF0018DefaultStyleKeyPropertyOverrideMetadataArgument);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(x => Handle(x), SyntaxKind.InvocationExpression);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is InvocationExpressionSyntax invocation &&
                DependencyProperty.TryGetOverrideMetadataCall(invocation, context.SemanticModel, context.CancellationToken, out var method) &&
                invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                context.SemanticModel.TryGetSymbol(memberAccess.Expression, context.CancellationToken, out ISymbol candidate) &&
                BackingFieldOrProperty.TryCreateForDependencyProperty(candidate, out var fieldOrProperty) &&
                method.TryFindParameter(KnownSymbol.PropertyMetadata, out var parameter) &&
                invocation.TryFindArgument(parameter, out var metadataArg))
            {
                if (fieldOrProperty.TryGetAssignedValue(context.CancellationToken, out var value) &&
                    value is InvocationExpressionSyntax registerInvocation)
                {
                    if (DependencyProperty.TryGetRegisterCall(registerInvocation, context.SemanticModel, context.CancellationToken, out var registerMethod) ||
                        DependencyProperty.TryGetRegisterReadOnlyCall(registerInvocation, context.SemanticModel, context.CancellationToken, out registerMethod) ||
                        DependencyProperty.TryGetRegisterAttachedCall(registerInvocation, context.SemanticModel, context.CancellationToken, out registerMethod) ||
                        DependencyProperty.TryGetRegisterAttachedReadOnlyCall(registerInvocation, context.SemanticModel, context.CancellationToken, out registerMethod))
                    {
                        if (registerMethod.TryFindParameter(KnownSymbol.PropertyMetadata, out var registerParameter) &&
                            registerInvocation.TryFindArgument(registerParameter, out var registeredMetadataArg) &&
                            context.SemanticModel.TryGetType(metadataArg.Expression, context.CancellationToken, out var type) &&
                            context.SemanticModel.TryGetType(registeredMetadataArg.Expression, context.CancellationToken, out var registeredType) &&
                            !type.IsAssignableTo(registeredType, context.Compilation))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0017MetadataMustBeAssignable, metadataArg.GetLocation()));
                        }
                    }
                }
                else if (fieldOrProperty.Symbol == KnownSymbol.FrameworkElement.DefaultStyleKeyProperty &&
                         metadataArg.Expression is ObjectCreationExpressionSyntax metadataCreation)
                {
                    if (!context.SemanticModel.TryGetSymbol(metadataCreation, KnownSymbol.FrameworkPropertyMetadata, context.CancellationToken, out _))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0017MetadataMustBeAssignable, metadataArg.GetLocation()));
                    }
                    else if (metadataCreation.TrySingleArgument(out var typeArg) &&
                             typeArg.Expression is TypeOfExpressionSyntax typeOf &&
                             typeOf.Type is IdentifierNameSyntax typeName &&
                             typeName.Identifier.ValueText != context.ContainingSymbol.ContainingType.MetadataName)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0018DefaultStyleKeyPropertyOverrideMetadataArgument, typeArg.GetLocation()));
                    }
                }
            }
        }
    }
}
