using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace PoorMansPaint.View.CustomCanvas
{
    public class LineDrawingTool : ShapeDrawingTool
    {
        public LineDrawingTool(CustomCanvas target) : base(target)
        {
        }

        protected override Geometry CreateShape(Point StartPoint, Point EndPoint)
        {
            return new LineGeometry(StartPoint, EndPoint);
        }
    }
}
