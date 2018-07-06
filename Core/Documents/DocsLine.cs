using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morenan.MRATextBox.Core.Documents
{
    internal class DocsLine : DocsItem, IDocsLine
    {
        public DocsLine(string _name, IDocsItem _baseon, IDocsItem _parent) : base(_name, _baseon, _parent)
        {

        }

        #region Number

        private IDocsKeyWord left;
        public IDocsKeyWord Left
        {
            get { return this.left ?? ((BaseOn is IDocsLine) ? ((IDocsLine)BaseOn).Left : null); }
            set { this.left = value; }
        }
        
        private IDocsColor fill;
        public IDocsColor Fill
        {
            get { return this.fill ?? ((BaseOn is IDocsLine) ? ((IDocsLine)BaseOn).Fill : null); }
            set { this.fill = value; }
        }

        private IList<IDocsItem> items;
        public IList<IDocsItem> Items
        {
            get { return this.items ?? ((BaseOn is IDocsLine) ? ((IDocsLine)BaseOn).Items : null); }
            set { this.items = value.ToList(); }
        }
        
        #endregion
    }
}
