#pragma warning disable 660,661 // using a hack with operator overloads
namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal class QualifiedMember<T>
        where T : ISymbol
    {
        internal readonly string Name;
        internal readonly QualifiedType ContainingType;

        public QualifiedMember(QualifiedType containingType, string name)
        {
            this.Name = name;
            this.ContainingType = containingType;
        }

        public static bool operator ==(BackingFieldOrProperty left, QualifiedMember<T> right)
        {
            if (left.Symbol is T symbol)
            {
                return symbol == right;
            }

            return false;
        }

        public static bool operator !=(BackingFieldOrProperty left, QualifiedMember<T> right)
        {
            return !(left == right);
        }

        public static bool operator ==(FieldOrProperty left, QualifiedMember<T> right)
        {
            if (left.Symbol is T symbol)
            {
                return symbol == right;
            }

            return false;
        }

        public static bool operator !=(FieldOrProperty left, QualifiedMember<T> right)
        {
            return !(left == right);
        }

        public static bool operator ==(T left, QualifiedMember<T> right)
        {
            if (left == null && right == null)
            {
                return true;
            }

            if (left == null || right == null)
            {
                return false;
            }

            return left.Name == right.Name &&
                   left.ContainingType == right.ContainingType;
        }

        public static bool operator !=(T left, QualifiedMember<T> right) => !(left == right);
    }
}