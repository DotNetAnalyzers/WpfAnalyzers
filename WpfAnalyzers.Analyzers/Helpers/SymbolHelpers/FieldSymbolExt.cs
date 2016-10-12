﻿namespace WpfAnalyzers
{
    using System.Threading;

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

            SyntaxReference reference;
            if (field.DeclaringSyntaxReferences.TryGetLast(out reference))
            {
                var declarator = reference.GetSyntax(cancellationToken) as VariableDeclaratorSyntax;
                value = declarator?.Initializer?.Value;
                return value != null;
            }

            return false;
        }
    }
}