using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Input;
using PoorMansPaint.CustomCanvas;

namespace PoorMansPaint.DrawingTools
{
    public class Pencil : DrawingTool
    {
        protected PathGeometry? _path;

        public override string MagicWord => "pencil";

        public override bool IsDrawing()
        {
            return _target != null && _path != null;
        }

        public override void StartDrawingAt(CustomCanvas.CustomCanvas target, Point pos)
        {
            if (!target.ContainsPoint(pos)) return;
            _target = target;

            _path = new PathGeometry();
            PathFigure figure = new PathFigure()
            {
                StartPoint = _target.GetRoundedPoint(pos),
            };
            Point tmp = new Point(figure.StartPoint.X, figure.StartPoint.Y);
            figure.Segments.Add(new LineSegment(tmp, true));
            _path.Figures.Add(figure);
            
            _target.Commander.Command(new DrawCommand(_path, _target.Pen));
        }

        public override void ContinueDrawingAt(Point pos)
        {
            if (!IsDrawing()) return;

            int lastSegmentIndex = _path.Figures.Count - 1;
            PathFigure figure = _path.Figures[lastSegmentIndex];
            if (figure.StartPoint.Equals(new Point(-1, -1)))
            {
                // (-1,-1) is 'null' point we assigned below
                // which means we start a new segment
                figure.StartPoint = pos;
            }

            // draw a line from the last point
            figure.Segments.Add(new LineSegment(
                _target.GetRoundedPoint(pos),
                true));
        }

        public override void FinishDrawing()
        {
            if (_path == null) return;
            _path.Freeze();
            _path = null;
        }

        public override object ButtonIcon { get; }
        public override Cursor Cursor => Cursors.Pen;
    }
}
