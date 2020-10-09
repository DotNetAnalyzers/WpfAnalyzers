// ReSharper disable All
namespace ValidCode.Repro
{
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Shapes;

    /// <summary>
    ///     Provides a base class for ArrowLine and ArrowPolyline.
    ///     This class is abstract.
    /// </summary>
    public abstract class ArrowLineBase : Shape
    {
        /// <summary>Identifies the <see cref="ArrowAngle"/> dependency property.</summary>
        public static readonly DependencyProperty ArrowAngleProperty = DependencyProperty.Register(
            nameof(ArrowAngle),
            typeof(double),
            typeof(ArrowLineBase),
            new FrameworkPropertyMetadata(
                45.0,
                FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>Identifies the <see cref="ArrowLength"/> dependency property.</summary>
        public static readonly DependencyProperty ArrowLengthProperty = DependencyProperty.Register(
            nameof(ArrowLength),
            typeof(double),
            typeof(ArrowLineBase),
            new FrameworkPropertyMetadata(
                12.0,
                FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>Identifies the <see cref="ArrowEnds"/> dependency property.</summary>
        public static readonly DependencyProperty ArrowEndsProperty = DependencyProperty.Register(
            nameof(ArrowEnds),
            typeof(ArrowEnds),
            typeof(ArrowLineBase),
            new FrameworkPropertyMetadata(
                ArrowEnds.End,
                FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>Identifies the <see cref="IsArrowClosed"/> dependency property.</summary>
        public static readonly DependencyProperty IsArrowClosedProperty = DependencyProperty.Register(
            nameof(IsArrowClosed),
            typeof(bool),
            typeof(ArrowLineBase),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.AffectsMeasure));

        private readonly PathFigure _pathfigHead1;
        private readonly PathFigure _pathfigHead2;

        /// <summary>
        ///     Initializes a new instance of ArrowLineBase.
        /// </summary>
        protected ArrowLineBase()
        {
            PathGeometry = new PathGeometry();

            PathfigureLine = new PathFigure();
            PolySegmentLine = new PolyLineSegment();
            PathfigureLine.Segments.Add(PolySegmentLine);

            _pathfigHead1 = new PathFigure();
            var polySegmentHead1 = new PolyLineSegment();
            _pathfigHead1.Segments.Add(polySegmentHead1);

            _pathfigHead2 = new PathFigure();
            var polysegHead2 = new PolyLineSegment();
            _pathfigHead2.Segments.Add(polysegHead2);
        }

        /// <summary>
        ///     Gets or sets the angle between the two sides of the arrowhead.
        /// </summary>
        public double ArrowAngle
        {
            get => (double)GetValue(ArrowAngleProperty);
            set => SetValue(ArrowAngleProperty, value);
        }

        /// <summary>
        ///     Gets or sets the length of the two sides of the arrowhead.
        /// </summary>
        public double ArrowLength
        {
            get => (double)GetValue(ArrowLengthProperty);
            set => SetValue(ArrowLengthProperty, value);
        }

        /// <summary>
        ///     Gets or sets the property that determines which ends of the
        ///     line have arrows.
        /// </summary>
        public ArrowEnds ArrowEnds
        {
            get => (ArrowEnds)GetValue(ArrowEndsProperty);
            set => SetValue(ArrowEndsProperty, value);
        }

        /// <summary>
        ///     Gets or sets the property that determines if the arrow head
        ///     is closed to resemble a triangle.
        /// </summary>
        public bool IsArrowClosed
        {
            get => (bool)GetValue(IsArrowClosedProperty);
            set => SetValue(IsArrowClosedProperty, value);
        }

        /// <summary>
        ///     Gets a value that represents the Geometry of the ArrowLine.
        /// </summary>
        protected override Geometry DefiningGeometry
        {
            get
            {
                var count = PolySegmentLine.Points.Count;

                if (count > 0)
                {
                    // Draw the arrow at the start of the line.
                    if ((ArrowEnds & ArrowEnds.Start) == ArrowEnds.Start)
                    {
                        var pt1 = PathfigureLine.StartPoint;
                        var pt2 = PolySegmentLine.Points[0];
                        PathGeometry.Figures.Add(CalculateArrow(_pathfigHead1, pt2, pt1));
                    }

                    // Draw the arrow at the end of the line.
                    if ((ArrowEnds & ArrowEnds.End) == ArrowEnds.End)
                    {
                        var pt1 = count == 1 ? PathfigureLine.StartPoint :
                                                 PolySegmentLine.Points[count - 2];
                        var pt2 = PolySegmentLine.Points[count - 1];
                        PathGeometry.Figures.Add(CalculateArrow(_pathfigHead2, pt1, pt2));
                    }
                }

                return PathGeometry;
            }
        }

        protected PathGeometry PathGeometry { get; set; }

        protected PathFigure PathfigureLine { get; set; }

        protected PolyLineSegment PolySegmentLine { get; set; }

        private PathFigure CalculateArrow(PathFigure pathfig, Point pt1, Point pt2)
        {
            var matrix = default(Matrix);
            var vector = pt1 - pt2;
            vector.Normalize();
            vector *= ArrowLength;

            if (pathfig.Segments[0] is PolyLineSegment polySegment)
            {
                polySegment.Points.Clear();
                matrix.Rotate(ArrowAngle / 2);
                pathfig.SetCurrentValue(PathFigure.StartPointProperty, pt2 + (vector * matrix));
                polySegment.Points.Add(pt2);

                matrix.Rotate(-ArrowAngle);
                polySegment.Points.Add(pt2 + (vector * matrix));
            }

            pathfig.SetCurrentValue(PathFigure.IsClosedProperty, IsArrowClosed);

            return pathfig;
        }
    }
}
