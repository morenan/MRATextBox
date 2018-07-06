using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morenan.MRATextBox.Core.Documents
{
    internal class DocsCycle : DocsItem, IDocsCycle
    {
        public DocsCycle(string _name, IDocsItem _baseonitem, IDocsItem _parentitem) : base(_name, _baseonitem, _parentitem)
        {
            this.items = null;
            this.ignorestart = null;
            this.ignoreend = null;
        }

        #region Number

        private List<IDocsItem> items;
        public IList<IDocsItem> Items
        {
            get { return this.items ?? ((BaseOn is IDocsCycle) ? ((IDocsCycle)BaseOn).Items : null); }
            set { this.items = value.ToList(); }
        }

        private int? ignorestart;
        public int IgnoreStart
        {
            get { return this.ignorestart ?? ((BaseOn is IDocsCycle) ? ((IDocsCycle)BaseOn).IgnoreStart : 0); }
            set { this.ignorestart = value; }
        }

        private int? ignoreend;
        public int IgnoreEnd
        {
            get { return this.ignoreend ?? ((BaseOn is IDocsCycle) ? ((IDocsCycle)BaseOn).IgnoreEnd : 0); }
            set { this.ignoreend = value; }
        }

        #endregion
    }
}
