using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Morenan.MRATextBox.Core.Documents
{
    internal class DocsWord : DocsItem, IDocsWord
    {
        public DocsWord(string _name, IDocsItem _baseon, IDocsItem _parent) : base(_name, _baseon, _parent)
        {

        }

        #region Number
        
        private Regex regex;
        public Regex Regex
        {
            get { return this.regex ?? ((BaseOn is IDocsWord) ? ((IDocsWord)BaseOn).Regex : null); }
            set { this.regex = value; }
        }
        
        private IDocsColor fill;
        public IDocsColor Fill
        {
            get { return this.fill ?? ((BaseOn is IDocsWord) ? ((IDocsWord)BaseOn).Fill : null); }
            set { this.fill = value; }
        }


        #endregion

    }
}
