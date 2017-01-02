namespace WpfAnalyzers.DependencyProperties
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
        private const string Title = "XmlnsDefinitions does not map all namespaces with public types.";
        private const string MessageFormat = "XmlnsDefinitions does not map all namespaces with public types.";
        private const string Description = "XmlnsDefinitions does not map all namespaces with public types.";
        private static readonly string HelpLink = WpfAnalyzers.HelpLink.ForId(DiagnosticId);

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            AnalyzerCategory.DependencyProperties,
            DiagnosticSeverity.Info,
            AnalyzerConstants.EnabledByDefault,
            Description,
            HelpLink);

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

            var attributeSyntax = context.Node as AttributeSyntax;
            if (attributeSyntax == null ||
                attributeSyntax.IsMissing)
            {
                return;
            }

            var type = context.SemanticModel.GetTypeInfoSafe(attributeSyntax, context.CancellationToken).Type;
            if (type != KnownSymbol.XmlnsDefinitionAttribute)
            {
                return;
            }

            using (var walker = Walker.Create(context.Compilation, context.SemanticModel, context.CancellationToken))
            {
                if (walker.Item.NotMapped.Count != 0)
                {
                    var missing = ImmutableDictionary.CreateRange(
                        walker.Item.NotMapped.Select(x => new KeyValuePair<string, string>(x, x)));
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, attributeSyntax.GetLocation(), missing));
                }
            }
        }

        private sealed class Walker : CSharpSyntaxWalker
        {
            private static readonly Pool<Walker> Pool = new Pool<Walker>(
                () => new Walker(),
                x =>
                    {
                        x.namespaces.Clear();
                        x.mappedNamespaces.Clear();
                        x.semanticModel = null;
                        x.cancellationToken = CancellationToken.None;
                    });

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

            public static Pool<Walker>.Pooled Create(Compilation compilation, SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                var pooled = Pool.GetOrCreate();
                pooled.Item.semanticModel = semanticModel;
                pooled.Item.cancellationToken = cancellationToken;
                foreach (var tree in compilation.SyntaxTrees)
                {
                    if (tree.FilePath.EndsWith(".g.cs"))
                    {
                        continue;
                    }

                    SyntaxNode root;
                    if (tree.TryGetRoot(out root))
                    {
                        pooled.Item.Visit(root);
                    }
                }

                return pooled;
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
                AttributeSyntax attribute;
                if (Attribute.TryGetAttribute(node, KnownSymbol.XmlnsDefinitionAttribute, this.semanticModel, this.cancellationToken, out attribute))
                {
                    AttributeArgumentSyntax arg;
                    if (Attribute.TryGetArgument(node, 1, KnownSymbol.XmlnsDefinitionAttribute.ClrNamespaceArgumentName, out arg))
                    {
                        string @namespace;
                        if (this.semanticModel.TryGetConstantValue(arg.Expression, this.cancellationToken, out @namespace))
                        {
                            this.mappedNamespaces.Add(@namespace);
                        }
                    }
                }
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

                    var xIdentifier = x as IdentifierNameSyntax;
                    var yIdentifier = y as IdentifierNameSyntax;
                    if (xIdentifier != null && yIdentifier != null)
                    {
                        return xIdentifier.Identifier.ValueText == yIdentifier.Identifier.ValueText;
                    }

                    var xName = x as QualifiedNameSyntax;
                    var yName = y as QualifiedNameSyntax;

                    if (xName != null && yName != null)
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