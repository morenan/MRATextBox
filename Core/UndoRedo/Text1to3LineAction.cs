using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morenan.MRATextBox.Core.UndoRedo
{
    internal class Text1to3LineAction : TextAction, IText1to3LineAction
    {
        #region Number

        private int sourceline;
        public int SourceLine { get { return this.sourceline; } set { this.sourceline = value; } }

        private int tolinenumber;
        public int ToLineNumber { get { return this.tolinenumber; } set { this.tolinenumber = value; } }

        private ITextZone zone;
        public ITextZone Zone { get { return this.zone; } set { this.zone = value; } }

        #endregion
    }
}
