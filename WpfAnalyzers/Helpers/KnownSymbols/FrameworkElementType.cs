namespace WpfAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;

    internal class FrameworkElementType : QualifiedType
    {
        internal readonly QualifiedField DataContextProperty;
        internal readonly QualifiedField StyleProperty;
        internal readonly QualifiedField DefaultStyleKeyProperty;

        internal readonly QualifiedProperty DataContext;
        internal readonly QualifiedProperty Style;

        internal FrameworkElementType()
            : base("System.Windows.FrameworkElement")
        {
            this.DataContextProperty = new QualifiedField(this,     nameof(this.DataContextProperty));
            this.DefaultStyleKeyProperty = new QualifiedField(this, nameof(this.DefaultStyleKeyProperty));
            this.StyleProperty = new QualifiedField(this, nameof(this.StyleProperty));

            this.DataContext = new QualifiedProperty(this, nameof(this.DataContext));
            this.Style = new QualifiedProperty(this, nameof(this.Style));
        }
    }
}
