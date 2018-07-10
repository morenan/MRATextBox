using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morenan.MRATextBox.Core.UndoRedo
{
    internal class TextIndentAction : TextAction, ITextIndentAction
    {
        #region Number

        private int indentstartline;
        public int IndentStartLine { get { return this.indentstartline; } set { this.indentstartline = value; } }

        private int[] indentspaces;
        public int[] IndentSpaces { get { return this.indentspaces; } set { this.indentspaces = value; } }

        private int[] indentindics;
        public int[] IndentIndics { get { return this.indentindics; } set { this.indentindics = value; } }

        #endregion
    }
}
