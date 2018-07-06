using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morenan.MRATextBox.Core.Documents
{
    internal class DocsZone : DocsItem, IDocsZone
    {
        public DocsZone(string _name, IDocsItem _baseon, IDocsItem _parent) : base(_name, _baseon, _parent)
        {

        }

        #region Number

        private IDocsKeyWord left;
        public IDocsKeyWord Left
        {
            get { return this.left ?? ((BaseOn is IDocsZone) ? ((IDocsZone)BaseOn).Left : null); }
            set { this.left = value; }
        }
        
        private IDocsKeyWord right;
        public IDocsKeyWord Right
        {
            get { return this.right ?? ((BaseOn is IDocsZone) ? ((IDocsZone)BaseOn).Right : null); }
            set { this.right = value; }
        }

        private IDocsColor fill;
        public IDocsColor Fill
        {
            get { return this.fill ?? ((BaseOn is IDocsZone) ? ((IDocsZone)BaseOn).Fill : null); }
            set { this.fill = value; }
        }

        private IList<IDocsItem> items;
        public IList<IDocsItem> Items
        {
            get { return this.items ?? ((BaseOn is IDocsZone) ? ((IDocsZone)BaseOn).Items : null); }
            set { this.items = value.ToList(); }
        }
         
        #endregion
    }
}
