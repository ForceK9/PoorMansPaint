using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PoorMansPaint.CustomCanvas
{
    public abstract class ShapeDrawingTool : DrawingTool
    {
        public override Cursor Cursor => Cursors.Cross;
        public abstract object ToolTipContent { get; }

        protected GeometryGroup? _shape;
        protected Point? _startPoint;

        public ShapeDrawingTool()
        {
            _startPoint = null;
        }

        protected abstract Geometry CreateShape(Point StartPoint, Point EndPoint);

        public override void StartDrawingAt(CustomCanvas target, Point pos)
        {
            if (!target.ContainsPoint(pos)) return;
            _target = target;
            _startPoint = pos;
        }

        public override void ContinueDrawingAt(Point pos)
        {
            if (_startPoint == null) return;
            if (_shape == null)
            {
                _shape = new GeometryGroup();
                _target.Commander.Command(new DrawCommand(_shape, _target.Pen));
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
