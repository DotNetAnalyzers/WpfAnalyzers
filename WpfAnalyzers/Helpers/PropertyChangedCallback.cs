namespace WpfAnalyzers;

using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal readonly struct PropertyChangedCallback
{
    internal readonly IdentifierNameSyntax Identifier;
    internal readonly IMethodSymbol Target;

    internal PropertyChangedCallback(IdentifierNameSyntax identifier, IMethodSymbol target)
    {
        this.Identifier = identifier;
        this.Target = target;
    }

    internal static PropertyChangedCallback? Match(ArgumentSyntax callback, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        if (Callback.Match(callback, KnownSymbols.PropertyChangedCallback, semanticModel, cancellationToken) is { } match)
        {
            return new PropertyChangedCallback(match.Identifier, match.Target);
        }

        return null;
    }
}
