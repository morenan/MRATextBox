using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morenan.MRATextBox.Core
{
    internal class TextKeyCollection : ITextKeyCollection
    {
        public TextKeyCollection(ITextKeyCore _prototype)
        {
            this.prototype = _prototype;
            this.items = new List<ITextKeyCore>();
            items.Add(prototype);
        }

        #region Number

        private ITextKeyCore prototype;
        public ITextKeyCore Prototype { get { return this.prototype; } }

        private List<ITextKeyCore> items;
        public IList<ITextKeyCore> Items { get { return this.items; } }

        #endregion

    }
}
