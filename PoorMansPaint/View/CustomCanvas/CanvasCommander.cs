using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoorMansPaint.View.CustomCanvas
{
    // Use this class to perform undoable commands on CustomCanvas
    public class CanvasCommander
    {
        protected int _limit;
        protected FixedSizeStack<UndoableCommand> _normalHistory = new FixedSizeStack<UndoableCommand>(0);
        protected FixedSizeStack<UndoableCommand> _undoneHistory = new FixedSizeStack<UndoableCommand>(0);
         
        public CustomCanvas Target { get; protected set; }
        public int HistoryLimit
        {
            get { return _limit; }
            set
            {
                _limit = value;
                _normalHistory.Capacity = _limit;
                _undoneHistory.Capacity = _limit;
            }
        }
        public CanvasCommander(CustomCanvas target, int historyLimit = 30)
        {
            Target = target;
            HistoryLimit = historyLimit;
        }

        public bool IsUndoingPossible()
        {
            return _normalHistory.Count > 0;
        }

        public bool IsRedoingPossible()
        {
            return _undoneHistory.Count > 0;
        }

        // order Target to do something specified by UndoableCommand
        public void Command(UndoableCommand command)
        {
            _undoneHistory.Clear();
            _normalHistory.Push(command);
            command.Execute(Target);
        }

        // undo the last UndoableCommand
        public void Undo()
        {
            UndoableCommand? lastCommand = _normalHistory.Pop();
            if (lastCommand == null) return;
            _undoneHistory.Push(lastCommand);
            lastCommand.Undo();
        }

        // redo the last undid UndoableCommand
        public void Redo()
        {
            UndoableCommand? lastCommand = _undoneHistory.Pop();
            if (lastCommand == null) return; 
            _normalHistory.Push(lastCommand);
            lastCommand.Execute(Target);
        }
    }
}
