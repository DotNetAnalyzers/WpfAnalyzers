namespace WpfAnalyzers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0052XmlnsDefinitionsDoesNotMapAllNamespaces : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0052";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "XmlnsDefinitions does not map all namespaces with public types.",
            messageFormat: "XmlnsDefinitions does not map all namespaces with public types.\r\nThe following namespaces are not mapped:\r\n{0}",
            category: AnalyzerCategory.XmlnsDefinition,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "XmlnsDefinitions does not map all namespaces with public types.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(HandleDeclaration, SyntaxKind.Attribute);
        }

        private static void HandleDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.Node is AttributeSyntax attribute &&
                Attribute.IsType(attribute, KnownSymbol.XmlnsDefinitionAttribute, context.SemanticModel, context.CancellationToken))
            {
                using (var walker = Walker.Create(context.Compilation, context.SemanticModel, context.CancellationToken))
                {
                    if (walker.NotMapped.Count != 0)
                    {
                        var missing = ImmutableDictionary.CreateRange(walker.NotMapped.Select(x => new KeyValuePair<string, string>(x, x)));
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, attribute.GetLocation(), missing, string.Join(Environment.NewLine, walker.NotMapped)));
                    }
                }
            }
        }

        private sealed class Walker : PooledWalker<Walker>
        {
            private static readonly IReadOnlyList<string> Ignored = new[] { "Annotations", "Properties", "XamlGeneratedNamespace" };

            private readonly HashSet<NameSyntax> namespaces = new HashSet<NameSyntax>(NameSyntaxComparer.Default);
            private readonly List<string> mappedNamespaces = new List<string>();

            private SemanticModel semanticModel;
            private CancellationToken cancellationToken;

            private Walker()
            {
            }

            public IReadOnlyList<string> NotMapped
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

            public static Walker Create(Compilation compilation, SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                var walker = Borrow(() => new Walker());
                walker.semanticModel = semanticModel;
                walker.cancellationToken = cancellationToken;
                foreach (var tree in compilation.SyntaxTrees)
                {
                    if (tree.FilePath.EndsWith(".g.cs"))
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

            public override void VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                if (node.Modifiers.Any(SyntaxKind.PublicKeyword))
                {
                    var @namespace = node.FirstAncestorOrSelf<NamespaceDeclarationSyntax>();
                    if (@namespace == null)
                    {
                        return;
                    }

                    this.namespaces.Add(@namespace.Name);
                }
            }

            public override void VisitAttribute(AttributeSyntax node)
            {
                if (Attribute.IsType(node, KnownSymbol.XmlnsDefinitionAttribute, this.semanticModel, this.cancellationToken))
                {
                    if (Attribute.TryGetArgument(node, 1, KnownSymbol.XmlnsDefinitionAttribute.ClrNamespaceArgumentName, out var arg))
                    {
                        if (this.semanticModel.TryGetConstantValue(arg.Expression, this.cancellationToken, out string @namespace))
                        {
                            this.mappedNamespaces.Add(@namespace);
                        }
                    }
                }
            }

            protected override void Clear()
            {
                this.namespaces.Clear();
                this.mappedNamespaces.Clear();
                this.semanticModel = null;
                this.cancellationToken = CancellationToken.None;
            }

            private class NameSyntaxComparer : IEqualityComparer<NameSyntax>
            {
                public static readonly NameSyntaxComparer Default = new NameSyntaxComparer();

                private NameSyntaxComparer()
                {
                }

                public bool Equals(NameSyntax x, NameSyntax y)
                {
                    if (ReferenceEquals(x, y))
                    {
                        return true;
                    }

                    if (x == null || y == null)
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

                    if (x is IdentifierNameSyntax xIdentifier &&
                        y is IdentifierNameSyntax yIdentifier)
                    {
                        return xIdentifier.Identifier.ValueText == yIdentifier.Identifier.ValueText;
                    }

                    if (x is QualifiedNameSyntax xName &&
                        y is QualifiedNameSyntax yName)
                    {
                        return xName.Right.Identifier.ValueText == yName.Right.Identifier.ValueText &&
                               RecursiveEquals(xName.Left, yName.Left);
                    }

                    throw new NotImplementedException("Don't think we can ever get here.");
                }
            }
        }
    }
}