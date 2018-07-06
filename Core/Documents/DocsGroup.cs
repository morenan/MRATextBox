using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morenan.MRATextBox.Core.Documents
{
    internal class DocsGroup : DocsItem, IDocsGroup
    {
        public DocsGroup(string _name, IDocsItem _baseon, IDocsItem _parent) : base(_name, _baseon, _parent)
        {

        }

        #region Number

        private List<IDocsItem> items;
        public IList<IDocsItem> Items
        {
            get { return this.items ?? ((BaseOn is IDocsGroup) ? ((IDocsGroup)BaseOn).Items : null); }
            set { this.items = value.ToList(); }
        }

        #endregion
    }
}
