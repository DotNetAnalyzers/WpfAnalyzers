#pragma warning disable 660,661 // using a hack with operator overloads
namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    [System.Diagnostics.DebuggerDisplay("{FullName}")]
    internal class QualifiedType
    {
        internal readonly string FullName;
        internal readonly NamespaceParts Namespace;
        internal readonly string Type;

        internal QualifiedType(string qualifiedName)
            : this(qualifiedName, NamespaceParts.Create(qualifiedName), qualifiedName.Substring(qualifiedName.LastIndexOf('.') + 1))
        {
        }

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
    }
}