namespace WpfAnalyzers;

using Gu.Roslyn.AnalyzerExtensions;

internal class DependencyPropertyKeyType : QualifiedType
{
    internal readonly QualifiedProperty DependencyProperty;

    internal DependencyPropertyKeyType()
        : base("System.Windows.DependencyPropertyKey")
    {
        this.DependencyProperty = new QualifiedProperty(this, nameof(this.DependencyProperty));
    }
}
