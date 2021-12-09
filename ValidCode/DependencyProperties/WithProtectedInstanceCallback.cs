namespace ValidCode.DependencyProperties
{
    using System.Windows;

    public class WithProtectedInstanceCallback : FrameworkElement
    {
        /// <summary>Identifies the <see cref="Number"/> dependency property.</summary>
        public static readonly DependencyProperty NumberProperty = DependencyProperty.Register(
            nameof(Number),
            typeof(int),
            typeof(WithProtectedInstanceCallback),
            new PropertyMetadata(
                default(int),
                (d, e) => ((WithProtectedInstanceCallback)d).OnNumberChanged()));

        private int n;

        public int Number
        {
            get => (int)this.GetValue(NumberProperty);
            set => this.SetValue(NumberProperty, value);
        }

        public int N() => this.n;

        /// <summary>This method is invoked when the <see cref="NumberProperty"/> changes.</summary>
        protected virtual void OnNumberChanged()
        {
            n = 0;
        }
    }
}
