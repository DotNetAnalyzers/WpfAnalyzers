namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class MethodDeclarationAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            WPF0004ClrMethodShouldMatchRegisteredName.Descriptor,
            WPF0042AvoidSideEffectsInClrAccessors.Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Handle, SyntaxKind.MethodDeclaration);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.Node is MethodDeclarationSyntax methodDeclaration &&
                context.ContainingSymbol is IMethodSymbol method &&
                method.IsStatic)
            {
                if (ClrMethod.IsAttachedSetMethod(methodDeclaration, context.SemanticModel, context.CancellationToken, out var call, out var fieldOrProperty))
                {
                    if (DependencyProperty.TryGetRegisteredName(fieldOrProperty, context.SemanticModel, context.CancellationToken, out var registeredName) &&
                        !method.Name.IsParts("Set", registeredName))
                    {
                        context.ReportDiagnostic(
                        Diagnostic.Create(
                            WPF0004ClrMethodShouldMatchRegisteredName.Descriptor,
                            methodDeclaration.Identifier.GetLocation(),
                            method.Name,
                            "Set" + registeredName));
                    }
                }
                else if (ClrMethod.IsAttachedGetMethod(methodDeclaration, context.SemanticModel, context.CancellationToken, out call, out fieldOrProperty))
                {
                    if (DependencyProperty.TryGetRegisteredName(fieldOrProperty, context.SemanticModel, context.CancellationToken, out var registeredName) &&
                        !method.Name.IsParts("Get", registeredName))
                    {
                        context.ReportDiagnostic(
                        Diagnostic.Create(
                            WPF0004ClrMethodShouldMatchRegisteredName.Descriptor,
                            methodDeclaration.Identifier.GetLocation(),
                            method.Name,
                            "Get" + registeredName));
                    }
                }

                if (call != null &&
                    methodDeclaration.Body is BlockSyntax body &&
                    body.Statements.TryGetFirst(x => !x.Contains(call), out var statement))
                {
                    context.ReportDiagnostic(Diagnostic.Create(WPF0042AvoidSideEffectsInClrAccessors.Descriptor, statement.GetLocation()));
                }
            }
        }
    }
}