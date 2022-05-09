namespace WpfAnalyzers;

using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal readonly struct CoerceValueCallback
{
    internal readonly IdentifierNameSyntax Identifier;
    internal readonly IMethodSymbol Target;

    internal CoerceValueCallback(IdentifierNameSyntax identifier, IMethodSymbol target)
    {
        this.Identifier = identifier;
        this.Target = target;
    }

    internal static CoerceValueCallback? Match(ArgumentSyntax callback, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        if (Callback.Match(callback, KnownSymbols.CoerceValueCallback, semanticModel, cancellationToken) is { } match)
        {
            return new CoerceValueCallback(match.Identifier, match.Target);
        }

        return null;
    }
}
