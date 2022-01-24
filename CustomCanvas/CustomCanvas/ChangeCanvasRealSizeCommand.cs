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
            _backup = target.DrawingGroup.Clone();
            target.RealWidth = Width; 
            target.RealHeight = Height;
            target.AddClipping();
        }

        public override void Undo()
        {
            if (_target == null) throw new NullReferenceException();
            _target.DrawingGroup = _backup;
        }
    }
}
