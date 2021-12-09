namespace WpfAnalyzers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;

    using Gu.Roslyn.AnalyzerExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0052XmlnsDefinitionsDoesNotMapAllNamespaces : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptors.WPF0052XmlnsDefinitionsDoesNotMapAllNamespaces);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(x => Handle(x), SyntaxKind.Attribute);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is AttributeSyntax attribute &&
                context.SemanticModel.TryGetNamedType(attribute, KnownSymbols.XmlnsDefinitionAttribute, context.CancellationToken, out _))
            {
                using var walker = Walker.Create(context.SemanticModel.Compilation, context.SemanticModel, context.CancellationToken);
                if (walker.NotMapped.Count != 0)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptors.WPF0052XmlnsDefinitionsDoesNotMapAllNamespaces,
                            attribute.GetLocation(),
                            ImmutableDictionary.CreateRange(walker.NotMapped.Select(x => new KeyValuePair<string, string?>(x, x))),
                            string.Join(Environment.NewLine, walker.NotMapped)));
                }
            }
        }

        private sealed class Walker : PooledWalker<Walker>
        {
            private static readonly IReadOnlyList<string> Ignored = new[] { "Annotations", "Properties", "XamlGeneratedNamespace" };

            private readonly HashSet<NameSyntax> namespaces = new(NameSyntaxComparer.Default);
            private readonly List<string> mappedNamespaces = new();

            private SemanticModel semanticModel = null!;
            private CancellationToken cancellationToken;

            private Walker()
            {
            }

            internal IReadOnlyList<string> NotMapped
            {
                get
                {
                    var notMapped = new List<string>();
                    foreach (var nameSyntax in this.namespaces)
                    {
                        var @namespace = nameSyntax.ToString();
                        if (this.mappedNamespaces.Contains(@namespace) ||
                            Ignored.Contains(@namespace))
                        {
                            continue;
                        }

                        notMapped.Add(@namespace);
                    }

                    return notMapped;
                }
            }

            public override void VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                if (node.Modifiers.Any(SyntaxKind.PublicKeyword))
                {
                    var @namespace = node.FirstAncestorOrSelf<NamespaceDeclarationSyntax>();
                    if (@namespace is null)
                    {
                        return;
                    }

                    this.namespaces.Add(@namespace.Name);
                }
            }

            public override void VisitAttribute(AttributeSyntax node)
            {
                if (this.semanticModel.TryGetNamedType(node, KnownSymbols.XmlnsDefinitionAttribute, this.cancellationToken, out _))
                {
                    if (node.TryFindArgument(1, KnownSymbols.XmlnsDefinitionAttribute.ClrNamespaceArgumentName, out var arg))
                    {
                        if (this.semanticModel.TryGetConstantValue<string>(arg.Expression, this.cancellationToken, out var @namespace))
                        {
                            this.mappedNamespaces.Add(@namespace);
                        }
                    }
                }
            }

            internal static Walker Create(Compilation compilation, SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                var walker = Borrow(() => new Walker());
                walker.semanticModel = semanticModel;
                walker.cancellationToken = cancellationToken;
                foreach (var tree in compilation.SyntaxTrees)
                {
                    if (tree.FilePath.EndsWith(".g.cs", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (tree.TryGetRoot(out var root))
                    {
                        walker.Visit(root);
                    }
                }

                return walker;
            }

            protected override void Clear()
            {
                this.namespaces.Clear();
                this.mappedNamespaces.Clear();
                this.semanticModel = null!;
                this.cancellationToken = CancellationToken.None;
            }

            private sealed class NameSyntaxComparer : IEqualityComparer<NameSyntax>
            {
                internal static readonly NameSyntaxComparer Default = new();

                private NameSyntaxComparer()
                {
                }

                public bool Equals(NameSyntax? x, NameSyntax? y)
                {
                    if (ReferenceEquals(x, y))
                    {
                        return true;
                    }

                    if (x is null || y is null)
                    {
                        return false;
                    }

                    return RecursiveEquals(x, y);
                }

                public int GetHashCode(NameSyntax obj) => 0;

                private static bool RecursiveEquals(NameSyntax x, NameSyntax y)
                {
                    if (x.GetType() != y.GetType())
                    {
                        return false;
                    }

                    if (x is IdentifierNameSyntax { Identifier: { ValueText: { } xText } } &&
                        y is IdentifierNameSyntax { Identifier: { ValueText: { } yText } })
                    {
                        return xText == yText;
                    }

                    if (x is QualifiedNameSyntax { Left: { } xLeft, Right: { Identifier: { ValueText: { } xRight } } } &&
                        y is QualifiedNameSyntax { Left: { } yLeft, Right: { Identifier: { ValueText: { } yRight } } })
                    {
                        return xRight == yRight &&
                               RecursiveEquals(xLeft, yLeft);
                    }

                    throw new NotSupportedException("Don't think we can ever get here.");
                }
            }
        }
    }
}
