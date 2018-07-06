using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morenan.MRATextBox.Core.Documents
{
    internal class DocsCompletation : DocsItem, IDocsCompletation
    {
        public DocsCompletation(string _name, IDocsItem _baseon, IDocsItem _parent) : base(_name, _baseon, _parent)
        {

        }
    }
}
