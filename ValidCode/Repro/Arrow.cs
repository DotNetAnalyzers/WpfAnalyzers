// ReSharper disable All
namespace ValidCode.Repro
{
    using System.Windows;
    using System.Windows.Media;

    public class Arrow : ArrowLineBase
    {
        /// <summary>Identifies the <see cref="Start"/> dependency property.</summary>
        public static readonly DependencyProperty StartProperty = DependencyProperty.Register(
            nameof(Start),
            typeof(Point),
            typeof(Arrow),
            new FrameworkPropertyMetadata(
                default(Point),
                FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>Identifies the <see cref="End"/> dependency property.</summary>
        public static readonly DependencyProperty EndProperty = DependencyProperty.Register(
            nameof(End),
            typeof(Point),
            typeof(Arrow),
            new FrameworkPropertyMetadata(
                default(Point),
                FrameworkPropertyMetadataOptions.AffectsMeasure));

        public Point Start
        {
            get => (Point)GetValue(StartProperty);
            set => SetValue(StartProperty, value);
        }

        public Point End
        {
            get => (Point)GetValue(EndProperty);
            set => SetValue(EndProperty, value);
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                // Clear out the PathGeometry.
                PathGeometry.Figures.Clear();

                if (IsNan(Start) || IsNan(End))
                {
                    return Geometry.Empty;
                }

                // Define a single PathFigure with the points.
                PathfigureLine.SetCurrentValue(PathFigure.StartPointProperty, Start);
                PolySegmentLine.Points.Clear();
                PolySegmentLine.Points.Add(End);
                PathGeometry.Figures.Add(PathfigureLine);

                // Call the base property to add arrows on the ends.
                return base.DefiningGeometry;

                static bool IsNan(Point p) => double.IsNaN(p.X) || double.IsNaN(p.Y);
            }
        }
    }
}
