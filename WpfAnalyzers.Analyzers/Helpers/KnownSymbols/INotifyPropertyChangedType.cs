namespace WpfAnalyzers
{
    using System.Diagnostics.CodeAnalysis;
    using Gu.Roslyn.AnalyzerExtensions;

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal class INotifyPropertyChangedType : QualifiedType
    {
        internal readonly QualifiedEvent PropertyChanged;

        internal INotifyPropertyChangedType()
            : base("System.ComponentModel.INotifyPropertyChanged")
        {
            this.PropertyChanged = new QualifiedEvent(this, nameof(this.PropertyChanged));
        }
    }
}
