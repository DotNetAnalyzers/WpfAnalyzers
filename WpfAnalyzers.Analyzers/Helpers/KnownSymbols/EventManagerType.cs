namespace WpfAnalyzers
{
    internal class EventManagerType : QualifiedType
    {
        internal readonly QualifiedMethod RegisterClassHandler;
        internal readonly QualifiedMethod RegisterRoutedEvent;

        internal EventManagerType()
            : base("System.Windows.EventManager")
        {
            this.RegisterClassHandler = new QualifiedMethod(this, nameof(this.RegisterClassHandler));
            this.RegisterRoutedEvent = new QualifiedMethod(this, nameof(this.RegisterRoutedEvent));
        }
    }
}