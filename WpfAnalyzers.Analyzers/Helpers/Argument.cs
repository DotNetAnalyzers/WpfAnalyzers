namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class Argument
    {
        internal static bool TryGetArgument(ImmutableArray<IParameterSymbol> parameters, ArgumentListSyntax arguments, QualifiedType type, out ArgumentSyntax argument)
        {
            argument = null;
            if (parameters == null ||
                arguments == null)
            {
                return false;
            }

            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                if (parameter.Type == type)
                {
                    if (arguments.Arguments.TryGetAtIndex(i, out argument))
                    {
                        if (argument.NameColon == null)
                        {
                            return true;
                        }
                    }

                    foreach (var candidate in arguments.Arguments)
                    {
                        if (candidate.NameColon?.Name?.Identifier.ValueText == parameter.Name)
                        {
                            argument = candidate;
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
