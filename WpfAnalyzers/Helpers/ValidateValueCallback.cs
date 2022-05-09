namespace WpfAnalyzers;

using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal readonly struct ValidateValueCallback
{
    internal readonly IdentifierNameSyntax Identifier;
    internal readonly IMethodSymbol Target;

    internal ValidateValueCallback(IdentifierNameSyntax identifier, IMethodSymbol target)
    {
        this.Identifier = identifier;
        this.Target = target;
    }

    internal static ValidateValueCallback? Match(ArgumentSyntax callback, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        if (Callback.Match(callback, KnownSymbols.ValidateValueCallback, semanticModel, cancellationToken) is { } match)
        {
            return new ValidateValueCallback(match.Identifier, match.Target);
        }

        return null;
    }
}
