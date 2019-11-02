namespace WpfAnalyzers
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class FieldSymbolExt
    {
        internal static bool TryGetAssignedValue(this IFieldSymbol field, CancellationToken cancellationToken, [NotNullWhen(true)] out ExpressionSyntax? value)
        {
            if (field.TrySingleDeclaration(cancellationToken, out var declaration) &&
                declaration is { Declaration: { Variables: { } variables } } &&
                variables.TryLast(out var variable) &&
                variable is { Initializer: { Value: { } temp } })
            {
                value = temp;
                return true;
            }

            value = null;
            return false;
        }
    }
}
