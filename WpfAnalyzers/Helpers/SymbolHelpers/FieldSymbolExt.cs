namespace WpfAnalyzers
{
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class FieldSymbolExt
    {
        internal static bool TryGetAssignedValue(this IFieldSymbol field, CancellationToken cancellationToken, out ExpressionSyntax value)
        {
            value = null;
            if (field == null)
            {
                return false;
            }

            if (field.DeclaringSyntaxReferences.TrySingle(out var reference))
            {
                var declarator = reference.GetSyntax(cancellationToken) as VariableDeclaratorSyntax;
                value = declarator?.Initializer?.Value;
                return value != null;
            }

            return false;
        }
    }
}
