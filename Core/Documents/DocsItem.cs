using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morenan.MRATextBox.Core.Documents
{
    internal class DocsItem
    {
        public DocsItem(string _name, IDocsItem _baseon, IDocsItem _parent)
        {
            this.name = _name;
            this.baseon = _baseon;
            this.parent = _parent;
        }

        #region Number

        private string name;
        public string Name { get { return this.name; } set { this.name = value; } }

        private IDocsItem baseon;
        public IDocsItem BaseOn { get { return this.baseon; } set { this.baseon = value; } }

        private IDocsItem parent;
        public IDocsItem Parent { get { return this.parent; } set { this.parent = value; } }

        #endregion
    }
}
