namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class OverrideMetadataAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            WPF0017MetadataMustBeAssignable.Descriptor,
            WPF0018DefaultStyleKeyPropertyOverrideMetadataArgument.Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Handle, SyntaxKind.InvocationExpression);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.Node is InvocationExpressionSyntax invocation &&
                DependencyProperty.TryGetOverrideMetadataCall(invocation, context.SemanticModel, context.CancellationToken, out var method) &&
                invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                BackingFieldOrProperty.TryCreate(context.SemanticModel.GetSymbolSafe(memberAccess.Expression, context.CancellationToken), out var fieldOrProperty) &&
                Argument.TryGetArgument(method.Parameters, invocation.ArgumentList, KnownSymbol.PropertyMetadata, out var metadataArg))
            {
                if (fieldOrProperty.TryGetAssignedValue(context.CancellationToken, out var value) &&
                    value is InvocationExpressionSyntax registerInvocation)
                {
                    if (DependencyProperty.TryGetRegisterCall(registerInvocation, context.SemanticModel, context.CancellationToken, out var registerMethod) ||
                        DependencyProperty.TryGetRegisterReadOnlyCall(registerInvocation, context.SemanticModel, context.CancellationToken, out registerMethod) ||
                        DependencyProperty.TryGetRegisterAttachedCall(registerInvocation, context.SemanticModel, context.CancellationToken, out registerMethod) ||
                        DependencyProperty.TryGetRegisterAttachedReadOnlyCall(registerInvocation, context.SemanticModel, context.CancellationToken, out registerMethod))
                    {
                        if (Argument.TryGetArgument(registerMethod.Parameters, registerInvocation.ArgumentList, KnownSymbol.PropertyMetadata, out var registeredMetadataArg))
                        {
                            var type = context.SemanticModel.GetTypeInfoSafe(metadataArg.Expression, context.CancellationToken).Type;
                            var registeredType = context.SemanticModel.GetTypeInfoSafe(registeredMetadataArg.Expression, context.CancellationToken).Type;
                            if (type != null &&
                                registeredType != null &&
                                !type.Is(registeredType))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(WPF0017MetadataMustBeAssignable.Descriptor, metadataArg.GetLocation()));
                            }
                        }
                    }
                }
                else if (fieldOrProperty == KnownSymbol.FrameworkElement.DefaultStyleKeyProperty &&
                         metadataArg.Expression is ObjectCreationExpressionSyntax metadataCreation)
                {
                    if (!Constructor.TryGet(metadataCreation, KnownSymbol.FrameworkPropertyMetadata, context.SemanticModel, context.CancellationToken, out _))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(WPF0017MetadataMustBeAssignable.Descriptor, metadataArg.GetLocation()));
                    }
                    else if (metadataCreation.TrySingleArgument(out var typeArg) &&
                             typeArg.Expression is TypeOfExpressionSyntax typeOf &&
                             typeOf.Type is IdentifierNameSyntax typeName &&
                             typeName.Identifier.ValueText != context.ContainingSymbol.ContainingType.MetadataName)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(WPF0018DefaultStyleKeyPropertyOverrideMetadataArgument.Descriptor, typeArg.GetLocation()));
                    }
                }
            }
        }
    }
}