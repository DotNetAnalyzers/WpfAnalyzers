namespace WpfAnalyzers
{
    internal class DependencyObjectType : QualifiedType
    {
        internal readonly QualifiedMethod GetValue;
        internal readonly QualifiedMethod SetValue;
        internal readonly QualifiedMethod SetCurrentValue;

        internal DependencyObjectType()
            : base("System.Windows.DependencyObject")
        {
            this.GetValue = new QualifiedMethod(this, "GetValue");
            this.SetValue = new QualifiedMethod(this, "SetValue");
            this.SetCurrentValue = new QualifiedMethod(this, "SetCurrentValue");
        }
    }
}