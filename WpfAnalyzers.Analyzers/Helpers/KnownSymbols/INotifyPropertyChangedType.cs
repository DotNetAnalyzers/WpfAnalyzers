namespace WpfAnalyzers
{
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