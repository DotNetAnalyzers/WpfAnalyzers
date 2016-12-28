namespace WpfAnalyzers
{
    internal class NullableType : QualifiedType
    {
        internal readonly QualifiedMethod Equals;

        internal NullableType()
            : base("System.Nullable")
        {
            this.Equals = new QualifiedMethod(this, nameof(this.Equals));
        }
    }
}