using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morenan.MRATextBox.Core
{
    internal class TextKey : TextItem, ITextKey
    {
        public TextKey(TextBoxCore _core, ITextKeyCore _keycore) : base(_core)
        {
            this.keycore = _keycore;
            Doc = keycore.Doc;
        }

        protected override void _Dispose()
        {
            this.keycore = null;
            base._Dispose();
        }

        public override string ToString()
        {
            return keycore.Keyword;
        }

        #region Number

        private ITextKeyCore keycore;
        public ITextKeyCore KeyCore { get { return this.keycore; } }

        #endregion
    }
}
