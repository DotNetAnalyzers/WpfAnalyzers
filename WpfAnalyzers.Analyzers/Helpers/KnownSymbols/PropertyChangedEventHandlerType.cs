namespace WpfAnalyzers
{
    internal class PropertyChangedEventHandlerType : QualifiedType
    {
        internal readonly QualifiedMethod Invoke;

        public PropertyChangedEventHandlerType()
            : base("System.ComponentModel.PropertyChangedEventHandler")
        {
            this.Invoke = new QualifiedMethod(this, "Invoke");
        }
    }
}