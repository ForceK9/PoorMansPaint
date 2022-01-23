using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Input;

namespace PoorMansPaint.CustomCanvas
{
    // if you want to draw something on the drawing canvas (a pencil, some shapes, ...)
    // subclassing this class and put the .dll in the same directory as the executable
    // a support class for CustomCanvas
    public abstract class DrawingTool
    {
        protected CustomCanvas? _target;
        public DrawingTool()
        {
            _target = null;
        }

        public virtual bool IsDrawing()
        {
            return _target != null;
        }

        public abstract string MagicWord { get; }
        public abstract object ButtonIcon { get; }
        public abstract Cursor Cursor { get; }
        public abstract void StartDrawingAt(CustomCanvas target, Point pos);
        public abstract void ContinueDrawingAt(Point pos);
        public abstract void FinishDrawing();
    }
}
