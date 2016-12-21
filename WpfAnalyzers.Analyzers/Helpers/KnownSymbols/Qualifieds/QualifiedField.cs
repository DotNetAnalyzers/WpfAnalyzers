namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal class QualifiedField : QualifiedMember<IFieldSymbol>
    {
        public QualifiedField(QualifiedType containingType, string name)
            : base(containingType, name)
        {
        }
    }
}