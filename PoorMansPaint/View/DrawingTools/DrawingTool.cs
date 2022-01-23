using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Input;

namespace PoorMansPaint.View.CustomCanvas
{
    // if you want to draw something on the drawing canvas (a pencil, some shapes, ...)
    // subclassing this class and put the .dll in the same directory as the executable
    // a support class for CustomCanvas
    public abstract class DrawingTool
    {
        protected CustomCanvas _target;
        public DrawingTool (CustomCanvas target)
        {
            _target = target;
        }

        public bool IsPointInsideTarget(Point pos)
        {
            return pos.X >= 0 && pos.Y >= 0 && pos.X <= _target.Width && pos.Y <= _target.Height;
        }
        public abstract object ButtonIcon { get; }
        public abstract Cursor Cursor { get; }
        public abstract void StartDrawingAt(Point pos);
        public abstract void ContinueDrawingAt(Point pos);
        public abstract void FinishDrawing();
    }
}
