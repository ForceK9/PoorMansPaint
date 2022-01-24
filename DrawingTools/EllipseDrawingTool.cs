using PoorMansPaint.CustomCanvas;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DrawingTools
{
    public class EllipseDrawingTool : ShapeDrawingTool
    {
        public override object ToolTipContent => "Ellipse";

        public override string MagicWord => "ellipse";

        public override object ButtonIcon => new Ellipse()
        {
            Width = 19, 
            Height = 16,
            Stroke = new SolidColorBrush(Colors.Black),
            StrokeThickness = 1,
        };

        protected override Geometry CreateShape(Point StartPoint, Point EndPoint)
        {
            return new EllipseGeometry(new Rect(StartPoint,EndPoint));
        }
    }
}
