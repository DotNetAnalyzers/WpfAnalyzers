namespace WpfAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;

    internal class FrameworkElementType : QualifiedType
    {
        internal readonly QualifiedProperty DataContext;
        internal readonly QualifiedField DataContextProperty;
        internal readonly QualifiedField DefaultStyleKeyProperty;

        internal FrameworkElementType()
            : base("System.Windows.FrameworkElement")
        {
            this.DataContext = new QualifiedProperty(this, nameof(this.DataContext));
            this.DataContextProperty = new QualifiedField(this, nameof(this.DataContextProperty));
            this.DefaultStyleKeyProperty = new QualifiedField(this, nameof(this.DefaultStyleKeyProperty));
        }
    }
}
