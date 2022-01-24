using PoorMansPaint.CustomCanvas;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DrawingTools
{
    public class RectangleDrawingTool : ShapeDrawingTool
    {
        public override object ToolTipContent => "Rectangle";

        public override string MagicWord => "rect";

        public override object ButtonIcon => new Rectangle()
        {
            Width = 18, 
            Height = 15,
            Stroke = new SolidColorBrush(Colors.Black),
            StrokeThickness = 1,
        };

        protected override Geometry CreateShape(Point StartPoint, Point EndPoint)
        {
            return new RectangleGeometry(new Rect(StartPoint,EndPoint));
        }
    }
}
