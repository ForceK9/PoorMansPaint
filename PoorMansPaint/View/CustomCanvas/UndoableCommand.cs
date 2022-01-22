using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoorMansPaint.View.CustomCanvas
{
    public abstract class UndoableCommand
    {
        protected CustomCanvas? _target;
        public virtual void Execute(CustomCanvas target)
        {
            _target = target;
            // do things to target here
        }
        public virtual void Undo()
        {
            if (_target == null) throw new NullReferenceException();
            // revert things done to target here
        }
    }
}
