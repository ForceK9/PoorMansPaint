using PoorMansPaint.CustomCanvas;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RhombusDrawingTool
{
    public class RhombusDrawingTool : ShapeDrawingTool
    {
        public override object ToolTipContent => "Rhombus";

        public override string MagicWord => "rhombus";

        public override object ButtonIcon => new Path()
        {
            Data = CreateShape(new Point(1, 1), new Point(19, 19)),
            Stroke = new SolidColorBrush(Colors.Black),
            StrokeThickness = 1,
        };

        protected override Geometry CreateShape(Point StartPoint, Point EndPoint)
        {
            PathGeometry path = new PathGeometry();
            PathFigure figure =  new PathFigure();
            Rect bound = new Rect(StartPoint, EndPoint);
            double midX = (bound.BottomRight.X + bound.TopLeft.X) / 2;
            double midY = (bound.BottomRight.Y + bound.TopLeft.Y) / 2;
            figure.StartPoint = new Point(midX, bound.Y);
            figure.Segments.Add(
                new LineSegment(new Point(bound.BottomRight.X, midY), true));
            figure.Segments.Add(
                new LineSegment(new Point(midX, bound.BottomRight.Y), true));
            figure.Segments.Add(
                new LineSegment(new Point(bound.X, midY), true));
            figure.Segments.Add(
                new LineSegment(figure.StartPoint, true));
            path.Figures.Add(figure);
            return path;
        }
    }
}
