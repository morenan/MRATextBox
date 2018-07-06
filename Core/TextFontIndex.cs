using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morenan.MRATextBox.Core
{
    internal class TextFontIndex : ITextFontIndex
    {
        public TextFontIndex(ITextFontCore _core, int _index)
        {
            this.core = _core;
            this.index = _index;
        }

        #region Number

        private ITextFontCore core;
        public ITextFontCore Core { get { return this.core; } }

        private int index;
        public int Index { get { return this.index; } }

        #endregion
    }
}
