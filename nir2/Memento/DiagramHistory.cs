using nir2.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nir2.Memento
{
    public class DiagramHistory
    {
        private Stack<DiagramMemento> undoStack = new Stack<DiagramMemento>();
        private Stack<DiagramMemento> redoStack = new Stack<DiagramMemento>();
        private DiagramOriginator originator;
        private const int MAX_HISTORY = 30;

        public bool CanUndo => undoStack.Count > 0;
        public bool CanRedo => redoStack.Count > 0;

        public DiagramHistory(DiagramOriginator originator)
        {
            this.originator = originator;
        }

        public void SaveState()
        {
            var memento = originator.CreateMemento(Model.UserElements);
            
            undoStack.Push(memento);

            while (undoStack.Count > MAX_HISTORY)
            {
                var oldest = undoStack.Last();
                undoStack = new Stack<DiagramMemento>(undoStack.Reverse().Skip(1).Reverse());
            }

            redoStack.Clear();
        }

        public bool Undo()
        {
            if (undoStack.Count == 0) return false;

            redoStack.Push(originator.CreateMemento(Model.UserElements));
            var previousState = undoStack.Pop();
            originator.RestoreMemento(previousState);

            return true;
        }

        public bool Redo()
        {
            if (redoStack.Count == 0) return false;

            undoStack.Push(originator.CreateMemento(Model.UserElements));
            var nextState = redoStack.Pop();
            originator.RestoreMemento(nextState);

            return true;
        }
    }
}
