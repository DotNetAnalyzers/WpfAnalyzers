namespace ValidCode.DependencyProperties
{
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Shapes;

    public class Issue282 : Shape
    {
        private readonly PathGeometry pathGeometry = new PathGeometry();

        /// <summary>Identifies the <see cref="IsTrue"/> dependency property.</summary>
        public static readonly DependencyProperty IsTrueProperty = DependencyProperty.Register(
            nameof(IsTrue),
            typeof(bool),
            typeof(Issue282), new PropertyMetadata(default(bool)));

        public bool IsTrue
        {
            get => (bool)this.GetValue(IsTrueProperty);
            set => this.SetValue(IsTrueProperty, value);
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                this.pathGeometry.Figures.Clear();
                return this.pathGeometry;
            }
        }
    }
}
