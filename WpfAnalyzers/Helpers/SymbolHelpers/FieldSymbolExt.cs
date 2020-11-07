namespace WpfAnalyzers
{
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class FieldSymbolExt
    {
        internal static ExpressionSyntax? Value(this IFieldSymbol field, CancellationToken cancellationToken)
        {
            if (field.TrySingleDeclaration(cancellationToken, out var declaration) &&
                declaration is { Declaration: { Variables: { } variables } } &&
                variables.TryLast(out var variable) &&
                variable is { Initializer: { Value: { } value } })
            {
                return value;
            }

            return null;
        }
    }
}
