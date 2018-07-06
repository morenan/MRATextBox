using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morenan.MRATextBox.Core.Documents
{
    internal class DocsKeyWord : DocsItem, IDocsKeyWord
    {
        public DocsKeyWord(string _name, IDocsItem _baseon, IDocsItem _parent) : base(_name, _baseon, _parent)
        {

        }

        #region Number

        private ITextKeyCore key;
        public ITextKeyCore Key
        {
            get { return this.key ?? ((BaseOn is IDocsKeyWord) ? ((IDocsKeyWord)BaseOn).Key : null); }
            set { this.key = value; }
        }

        private IDocsColor fill;
        public IDocsColor Fill
        {
            get { return this.fill ?? ((BaseOn is IDocsKeyWord) ? ((IDocsKeyWord)BaseOn).Fill : null); }
            set { this.fill = value; }
        }

        #endregion

    }
}
