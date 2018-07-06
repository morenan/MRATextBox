using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morenan.MRATextBox.Core.Documents
{
    internal class DocsKeyWordCollection : DocsItem, IDocsKeyWordCollection
    {
        public DocsKeyWordCollection(string _name, IDocsItem _baseon, IDocsItem _parent) : base(_name, _baseon, _parent)
        {

        }

        #region Number

        private List<IDocsKeyWord> keywords;
        public IList<IDocsKeyWord> KeyWords
        {
            get { return this.keywords ?? ((BaseOn is IDocsKeyWordCollection) ? ((IDocsKeyWordCollection)BaseOn).KeyWords : null); }
            set { this.keywords = value.ToList(); }
        }

        #endregion
    }
}
