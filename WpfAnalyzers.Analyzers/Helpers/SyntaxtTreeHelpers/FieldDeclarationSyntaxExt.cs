﻿namespace WpfAnalyzers
{
    using System;

    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class FieldDeclarationSyntaxExt
    {
        internal static string Name(this FieldDeclarationSyntax declaration)
        {
            VariableDeclaratorSyntax variable = null;
            if (declaration?.Declaration?.Variables.TrySingle(out variable) == true)
            {
                return variable.Identifier.ValueText;
            }

            throw new InvalidOperationException($"Could not get name of field {declaration}");
        }
    }
}