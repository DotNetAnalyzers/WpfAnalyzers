namespace WpfAnalyzers
{
    internal class EventManagerType : QualifiedType
    {
        internal readonly QualifiedMethod RegisterClassHandler;

        internal EventManagerType()
            : base("System.Windows.EventManager")
        {
            this.RegisterClassHandler = new QualifiedMethod(this, nameof(this.RegisterClassHandler));
        }
    }
}