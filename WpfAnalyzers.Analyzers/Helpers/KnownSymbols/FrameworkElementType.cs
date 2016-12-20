namespace WpfAnalyzers
{
    internal class FrameworkElementType : QualifiedType
    {
        internal readonly QualifiedProperty DataContext;
        internal readonly QualifiedField DataContextProperty;

        internal FrameworkElementType()
            : base("System.Windows.FrameworkElement")
        {
            this.DataContext = new QualifiedProperty(this, "DataContext");
            this.DataContextProperty = new QualifiedField(this, "DataContextProperty");
        }
    }
}