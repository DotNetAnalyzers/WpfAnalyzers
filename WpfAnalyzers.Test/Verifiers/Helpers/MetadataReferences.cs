// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace WpfAnalyzers.Test
{
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    /// <summary>
    /// Metadata references used to create test projects.
    /// </summary>
    internal static class MetadataReferences
    {
        internal static readonly MetadataReference Corlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location).WithAliases(ImmutableArray.Create("global", "corlib"));
        internal static readonly MetadataReference System = MetadataReference.CreateFromFile(typeof(System.Diagnostics.Debug).Assembly.Location).WithAliases(ImmutableArray.Create("global", "system"));
        internal static readonly MetadataReference SystemCore = MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);
        internal static readonly MetadataReference PresentationCore = MetadataReference.CreateFromFile(typeof(System.Windows.Media.Brush).Assembly.Location);
        internal static readonly MetadataReference PresentationFramework = MetadataReference.CreateFromFile(typeof(System.Windows.Controls.Control).Assembly.Location);
        internal static readonly MetadataReference WindowsBase = MetadataReference.CreateFromFile(typeof(System.Windows.Media.Matrix).Assembly.Location);
        internal static readonly MetadataReference SystemXaml = MetadataReference.CreateFromFile(typeof(System.Xaml.XamlLanguage).Assembly.Location);
        internal static readonly MetadataReference CSharpSymbols = MetadataReference.CreateFromFile(typeof(CSharpCompilation).Assembly.Location);
        internal static readonly MetadataReference CodeAnalysis = MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location);

        internal static MetadataReference[] All =>
            new[]
            {
                Corlib,
                System,
                SystemCore,
                PresentationCore,
                PresentationFramework,
                WindowsBase,
                SystemXaml,
                CSharpSymbols,
                CodeAnalysis,
            };
    }
}
