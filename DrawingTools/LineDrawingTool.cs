using PoorMansPaint.CustomCanvas;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PoorMansPaint.DrawingTools
{
    public class LineDrawingTool : ShapeDrawingTool
    {
        public override string MagicWord => "line";

        public override object ButtonIcon => new Line()
        {
            X1 = 1, Y1 = 1,
            X2 = 16, Y2 = 16,
            Stroke = new SolidColorBrush(Colors.Black),
            StrokeThickness = 1,
        };

        public override object ToolTipContent => "Line";

        protected override Geometry CreateShape(Point StartPoint, Point EndPoint)
        {
            return new LineGeometry(StartPoint, EndPoint);
        }
    }
}
