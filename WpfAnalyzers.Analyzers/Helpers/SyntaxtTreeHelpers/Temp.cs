namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Stuff to move to extensions.
    /// </summary>
    internal static class Temp
    {
        /// <summary>
        /// Find the matching parameter for the argument.
        /// </summary>
        /// <param name="method">The <see cref="BaseMethodDeclarationSyntax"/></param>
        /// <param name="name">The <see cref="ArgumentSyntax"/></param>
        /// <param name="parameter">The matching <see cref="ParameterSyntax"/></param>
        /// <returns>True if a matching parameter was found.</returns>
        public static bool TryFindParameter(this BaseMethodDeclarationSyntax method, string name, out ParameterSyntax parameter)
        {
            parameter = null;
            if (name == null ||
                method == null)
            {
                return false;
            }

            if (method.ParameterList is ParameterListSyntax parameterList)
            {
                foreach (var candidate in parameterList.Parameters)
                {
                    if (candidate.Identifier.ValueText == name)
                    {
                        parameter = candidate;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
