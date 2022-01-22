using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoorMansPaint.View.CustomCanvas
{
    public class ChangeCanvasRealSizeCommand : UndoableCommand
    {
        protected double _oldWidth, _oldHeight;
        public double Width { get; set; }
        public double Height { get; set; }

        public ChangeCanvasRealSizeCommand(double width, double height)
        {
            Width = width;
            Height = height;
        }

        public override void Execute(CustomCanvas target)
        {
            _target = target;
            _oldWidth = target.Width;
            _oldHeight = target.Height;
            target.RealWidth = Width; 
            target.RealHeight = Height;
        }

        public override void Undo()
        {
            if (_target == null) throw new NullReferenceException();
            _target.Width = _oldWidth;
            _target.Height = _oldHeight;
        }
    }
}
