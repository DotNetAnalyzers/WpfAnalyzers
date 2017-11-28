namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0017MetadataMustBeAssignable : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0017";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Metadata must be of same type or super type.",
            messageFormat: "Metadata must be of same type or super type.",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "When overriding metadata must be of the same type or subtype of the overridden property's metadata.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Descriptor);

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
                fieldOrProperty.TryGetValue(context.CancellationToken, out var value) &&
                value is InvocationExpressionSyntax registerInvocation &&
                Argument.TryGetArgument(method.Parameters, invocation.ArgumentList, KnownSymbol.PropertyMetadata, out var metadataArg))
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
                            context.ReportDiagnostic(Diagnostic.Create(Descriptor, metadataArg.GetLocation()));
                        }
                    }
                }
            }
        }
    }
}