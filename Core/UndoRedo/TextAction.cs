using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morenan.MRATextBox.Core.UndoRedo
{
    internal class TextAction : ITextAction
    {
        #region Number
        
        private int oldlinestart;
        public int OldLineStart { get { return this.oldlinestart; } set { this.oldlinestart = value; } }

        private int oldcolumnstart;
        public int OldColumnStart { get { return this.oldcolumnstart; } set { this.oldcolumnstart = value; } }

        private int oldlineend;
        public int OldLineEnd { get { return this.oldlineend; } set { this.oldlineend = value; } }

        private int oldcolumnend;
        public int OldColumnEnd { get { return this.oldcolumnend; } set { this.oldcolumnend = value; } }

        private int newlinestart;
        public int NewLineStart { get { return this.newlinestart; } set { this.newlinestart = value; } }

        private int newcolumnstart;
        public int NewColumnStart { get { return this.newcolumnstart; } set { this.newcolumnstart = value; } }

        private int newlineend;
        public int NewLineEnd { get { return this.newlineend; } set { this.newlineend = value; } }

        private int newcolumnend;
        public int NewColumnEnd { get { return this.newcolumnend; } set { this.newcolumnend = value; } }

        #endregion
    }
}
