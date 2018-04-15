namespace WpfAnalyzers
{
    internal class StringType : QualifiedType
    {
        internal readonly QualifiedMethod Format;
        internal readonly QualifiedField Empty;

        internal StringType()
            : base("System.String", "string")
        {
            this.Format = new QualifiedMethod(this, nameof(this.Format));
            this.Empty = new QualifiedField(this, nameof(this.Empty));
        }
    }
}