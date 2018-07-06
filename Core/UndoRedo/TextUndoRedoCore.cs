using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morenan.MRATextBox.Core.UndoRedo
{
    internal class TextUndoRedoCore : ITextUndoRedoCore
    {
        public TextUndoRedoCore(ITextBoxCore _parent)
        {
            this.parent = _parent;
            this.undos = new List<ITextUndoRedoAction>();
            this.redos = new List<ITextUndoRedoAction>();
        }

        #region Number

        private ITextBoxCore parent;
        public ITextBoxCore Parent { get { return this.parent; } }

        private List<ITextUndoRedoAction> undos;
        public IList<ITextUndoRedoAction> Undos { get { return this.undos; } }

        private List<ITextUndoRedoAction> redos;
        public IList<ITextUndoRedoAction> Redos { get { return this.redos; } }

        #endregion

        #region Method

        public void Clear()
        {
            undos.Clear();
            redos.Clear();
        }

        public void Add(ITextUndoRedoAction action)
        {
            ITextUndoRedoAction lastundo = undos.LastOrDefault();
            ITextUndoRedoAction concat = lastundo?.Concat(action);
            if (concat != null)
            {
                undos.RemoveAt(undos.Count() - 1);
                undos.Add(concat);
            }
            else
            {
                undos.Add(action);
            }
        }
        
        public ITextUndoRedoAction Undo()
        {
            ITextUndoRedoAction lastundo = undos.LastOrDefault();
            if (lastundo != null) redos.Add(lastundo);
            return lastundo;
        }

        public ITextUndoRedoAction Redo()
        {
            ITextUndoRedoAction lastredo = redos.LastOrDefault();
            if (lastredo != null) undos.Add(lastredo);
            return lastredo;
        }
        
        #endregion
    }
}
