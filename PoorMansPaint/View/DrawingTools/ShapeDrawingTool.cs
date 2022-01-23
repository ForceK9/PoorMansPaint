using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace PoorMansPaint.View.CustomCanvas
{
    public abstract class ShapeDrawingTool : DrawingTool
    {
        public override object ButtonIcon => null;

        public override Cursor Cursor => Cursors.Cross;

        protected GeometryGroup _shape;
        protected Point? _startPoint;

        protected ShapeDrawingTool(CustomCanvas target) : base(target)
        {
            _startPoint = null;
        }

        protected abstract Geometry CreateShape(Point StartPoint, Point EndPoint);

        public override void StartDrawingAt(Point pos)
        {
            if (!IsPointInsideTarget(pos)) return;
            _startPoint = pos;
        }

        public override void ContinueDrawingAt(Point pos)
        {
            if (_startPoint == null) return;
            if (_shape == null)
            {
                _shape = new GeometryGroup();
                _target.Commander.Command(new DrawCommand(_shape));
            }
            _shape.Children.Clear();
            _shape.Children.Add(CreateShape((Point)_startPoint, pos));
        }

        public override void FinishDrawing()
        {
            if (_shape == null) return;
            _shape.Freeze();
            _shape = null;
            _startPoint = null;
        }
    }
}
