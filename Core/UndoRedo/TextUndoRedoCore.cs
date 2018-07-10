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
            this.undos = new List<ITextAction>();
            this.redos = new List<ITextAction>();
        }

        #region Number

        private ITextBoxCore parent;
        public ITextBoxCore Parent { get { return this.parent; } }

        private List<ITextAction> undos;
        public IList<ITextAction> Undos { get { return this.undos; } }

        private List<ITextAction> redos;
        public IList<ITextAction> Redos { get { return this.redos; } }

        #endregion

        #region Method

        public void Clear()
        {
            undos.Clear();
            redos.Clear();
        }

        public void Add(ITextAction action)
        {
            ITextAction lastundo = undos.LastOrDefault();
            ITextAction concat = null;
            if (lastundo is ITextUndoRedoAction && action is ITextUndoRedoAction)
                concat = ((ITextUndoRedoAction)lastundo).Concat((ITextUndoRedoAction)action);
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
        
        public ITextAction Undo()
        {
            ITextAction lastundo = undos.LastOrDefault();
            if (lastundo != null) redos.Add(lastundo);
            return lastundo;
        }

        public ITextAction Redo()
        {
            ITextAction lastredo = redos.LastOrDefault();
            if (lastredo != null) undos.Add(lastredo);
            return lastredo;
        }
        
        #endregion
    }
}
