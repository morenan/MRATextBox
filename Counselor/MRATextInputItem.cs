using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Morenan.MRATextBox.Core;

namespace Morenan.MRATextBox.Counselor
{
    internal class MRATextInputItem : IMRATextItem
    {
        public MRATextInputItem(IMRATextInputContext _parent, ITextItem _item)
        {
            this.parent = _parent;
            this.item = _item;
        }

        public override string ToString()
        {
            return item.ToString();
        }

        #region Number

        private IMRATextInputContext parent;
        public IMRATextInputContext Parent { get { return this.parent; } }

        private ITextItem item;
        public ITextItem Item { get { return this.item; } }

        #endregion

        #region IMRATextItem

        public string Name { get { return "{MRATextInputContext_Item}"; } }

        IMRATextItem IMRATextItem.Parent { get { return this.parent; } }

        public MRATextItemTypes ItemType { get { return item is ITextZone ? MRATextItemTypes.InputContextZone : MRATextItemTypes.InputContextSingle; } }

        public MRATextItemFeatures Feature { get { return MRATextItemFeatures.None; } }

        #endregion

    }
}
