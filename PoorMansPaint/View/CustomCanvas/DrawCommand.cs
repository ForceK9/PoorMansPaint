using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PoorMansPaint.View.CustomCanvas
{
    public class DrawCommand : UndoableCommand
    {
        protected DrawingGroup _backup;
        public Geometry ThingToDraw { get; set; }
        public DrawCommand(Geometry pathToDraw)
        {
            ThingToDraw = pathToDraw;
        }
        public override void Execute(CustomCanvas target)
        {
            base.Execute(target);
            _backup = target.DrawingGroup.Clone();
            using (DrawingContext dgdc = target.DrawingGroup.Append())
            {
                target.DrawingGroup.Children.Add(new GeometryDrawing(null,
                    new Pen(Brushes.Black, 1)
                    {
                        StartLineCap = PenLineCap.Round,
                        EndLineCap = PenLineCap.Round,
                        LineJoin = PenLineJoin.Round,
                    },
                    ThingToDraw));
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
