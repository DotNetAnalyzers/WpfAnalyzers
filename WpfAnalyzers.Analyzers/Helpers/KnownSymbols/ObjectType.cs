namespace WpfAnalyzers
{
    internal class ObjectType : QualifiedType
    {
        internal readonly QualifiedMethod Equals;
        internal readonly QualifiedMethod ReferenceEquals;

        internal ObjectType()
            : base("System.Object")
        {
            this.Equals = new QualifiedMethod(this, nameof(this.Equals));
            this.ReferenceEquals = new QualifiedMethod(this, nameof(this.ReferenceEquals));
        }
    }
}