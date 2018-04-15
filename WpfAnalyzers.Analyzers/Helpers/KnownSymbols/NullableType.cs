namespace WpfAnalyzers
{
    internal class NullableType : QualifiedType
    {
        internal new readonly QualifiedMethod Equals;

        internal NullableType()
            : base("System.Nullable")
        {
            this.Equals = new QualifiedMethod(this, nameof(this.Equals));
        }
    }
}
