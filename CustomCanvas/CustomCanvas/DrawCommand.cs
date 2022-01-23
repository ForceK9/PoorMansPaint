using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PoorMansPaint.CustomCanvas
{
    public class DrawCommand : UndoableCommand
    {
        protected DrawingGroup _backup;
        public Geometry ThingToDraw { get; set; }
        public Pen PenToDrawWith { get; set; }
        public DrawCommand(Geometry pathToDraw, Pen pen)
        {
            ThingToDraw = pathToDraw;
            PenToDrawWith = pen.Clone();
        }
        public override void Execute(CustomCanvas target)
        {
            base.Execute(target);
            _backup = target.DrawingGroup.Clone();
            using (DrawingContext dgdc = target.DrawingGroup.Append())
            {
                target.DrawingGroup.Children.Add(new GeometryDrawing(
                    null, PenToDrawWith, ThingToDraw));
            }
        }
        public override void Undo()
        {
            base.Undo();
            _target.DrawingGroup = _backup;
            _target.InvalidateVisual();
        }
    }
}
