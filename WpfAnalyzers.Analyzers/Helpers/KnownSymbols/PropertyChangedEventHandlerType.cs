namespace WpfAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;

    internal class PropertyChangedEventHandlerType : QualifiedType
    {
        internal readonly QualifiedMethod Invoke;

        public PropertyChangedEventHandlerType()
            : base("System.ComponentModel.PropertyChangedEventHandler")
        {
            this.Invoke = new QualifiedMethod(this, nameof(this.Invoke));
        }
    }
}
