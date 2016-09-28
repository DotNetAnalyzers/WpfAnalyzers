namespace WpfAnalyzers.DependencyProperties
{
    using System;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class TypeSyntaxExt
    {
        internal static string Name(this TypeSyntax type)
        {
            var identifier = type as IdentifierNameSyntax;
            if (identifier != null)
            {
                return identifier?.Identifier.ValueText;
            }

            System.Diagnostics.Debugger.Break();
            throw new NotImplementedException($"Cannot get name of {type}");
        }
    }
}