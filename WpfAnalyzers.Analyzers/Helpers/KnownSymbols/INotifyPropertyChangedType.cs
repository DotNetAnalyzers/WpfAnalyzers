namespace WpfAnalyzers
{
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal class INotifyPropertyChangedType : QualifiedType
    {
        internal readonly QualifiedEvent PropertyChanged;

        internal INotifyPropertyChangedType()
            : base("System.ComponentModel.INotifyPropertyChanged")
        {
            this.PropertyChanged = new QualifiedEvent(this, "PropertyChanged");
        }
    }
}