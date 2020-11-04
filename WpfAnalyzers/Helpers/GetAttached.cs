namespace WpfAnalyzers
{
    using System.Threading;

    using Gu.Roslyn.AnalyzerExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal readonly struct GetAttached
    {
        internal readonly DependencyObject.GetValue GetValue;
        internal readonly BackingFieldOrProperty Backing;

        private GetAttached(DependencyObject.GetValue getValue, BackingFieldOrProperty backing)
        {
            this.GetValue = getValue;
            this.Backing = backing;
        }

        internal static GetAttached? Match(MethodDeclarationSyntax method, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (method is { ParameterList: { Parameters: { Count: 1 } parameters } } &&
                !method.ReturnType.IsVoid() &&
                method.Modifiers.Any(SyntaxKind.StaticKeyword) &&
                DependencyObject.GetValue.Find(MethodOrAccessor.Create(method), semanticModel, cancellationToken) is { Invocation: { } invocation } getValue &&
                InvokedOnParameter(parameters[0], invocation) &&
                semanticModel.TryGetSymbol(getValue.PropertyArgument.Expression, cancellationToken, out var symbol) &&
                BackingFieldOrProperty.TryCreateForDependencyProperty(symbol, out var backing))
            {
                return new GetAttached(getValue, backing);
            }

            return null;

            static bool InvokedOnParameter(ParameterSyntax parameter, InvocationExpressionSyntax invocation)
            {
                return invocation switch
                {
                    { Expression: MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax { Identifier: { ValueText: { } name } } } } => name == parameter.Identifier.ValueText,
                    _ => false,
                };
            }
        }
    }
}
