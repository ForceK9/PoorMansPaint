using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace PoorMansPaint.CustomCanvas
{
    public class ChangeCanvasRealSizeCommand : UndoableCommand
    {
        protected DrawingGroup _backup;
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
            _backup = target.DrawingGroup;
            _oldWidth = target.Width;
            _oldHeight = target.Height;
            target.RealWidth = Width; 
            target.RealHeight = Height;
            target.AddClipping();
        }

        public override void Undo()
        {
            if (_target == null) throw new NullReferenceException();
            _target.Width = _oldWidth;
            _target.Height = _oldHeight;
            _target.DrawingGroup = _backup;
        }
    }
}
