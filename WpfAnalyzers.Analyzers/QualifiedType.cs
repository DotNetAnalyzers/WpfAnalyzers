#pragma warning disable 660,661 // using a hack with operator overloads
namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;

    [System.Diagnostics.DebuggerDisplay("{FullName}")]
    internal class QualifiedType
    {
        internal static readonly QualifiedType Object = Create("System.Object");
        internal static readonly QualifiedType DependencyObject = Create("System.Windows.DependencyObject");
        internal static readonly QualifiedType DependencyProperty = Create("System.Windows.DependencyProperty");
        internal static readonly QualifiedType DependencyPropertyKey = Create("System.Windows.DependencyPropertyKey");
        internal static readonly QualifiedType Freezable = Create("System.Windows.Freezable");

        internal static readonly QualifiedType XmlnsPrefixAttribute = Create("System.Windows.Markup.XmlnsPrefixAttribute");
        internal static readonly QualifiedType XmlnsDefinitionAttribute = Create("System.Windows.Markup.XmlnsDefinitionAttribute");

        internal readonly string FullName;
        internal readonly NamespaceParts Namespace;
        internal readonly string Type;

        private QualifiedType(string fullName, NamespaceParts @namespace, string type)
        {
            this.FullName = fullName;
            this.Namespace = @namespace;
            this.Type = type;
        }

        public static bool operator ==(ITypeSymbol left, QualifiedType right)
        {
            if (left == null && right == null)
            {
                return true;
            }

            if (left == null || right == null)
            {
                return false;
            }

            return left.Name == right.Type &&
                   left.ContainingNamespace == right.Namespace;
        }

        public static bool operator !=(ITypeSymbol left, QualifiedType right) => !(left == right);

        private static QualifiedType Create(string qualifiedName)
        {
            var parts = qualifiedName.Split('.').ToImmutableList();
            System.Diagnostics.Debug.Assert(parts.Count != 0, "parts.Length != 0");
            return new QualifiedType(qualifiedName, new NamespaceParts(parts.RemoveAt(parts.Count - 1)), parts.Last());
        }

        [System.Diagnostics.DebuggerDisplay("{System.string.Join(\".\", parts)}")]
        internal class NamespaceParts
        {
            private readonly ImmutableList<string> parts;

            public NamespaceParts(ImmutableList<string> parts)
            {
                this.parts = parts;
            }

            public static bool operator ==(INamespaceSymbol left, NamespaceParts right)
            {
                if (left == null && right == null)
                {
                    return true;
                }

                if (left == null || right == null)
                {
                    return false;
                }

                var ns = left;
                for (var i = right.parts.Count - 1; i >= 0; i--)
                {
                    if (ns == null || ns.IsGlobalNamespace)
                    {
                        return false;
                    }

                    if (ns.Name != right.parts[i])
                    {
                        return false;
                    }

                    ns = ns.ContainingNamespace;
                }

                return ns?.IsGlobalNamespace == true;
            }

            public static bool operator !=(INamespaceSymbol left, NamespaceParts right) => !(left == right);
        }
    }
}